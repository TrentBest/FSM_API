using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests.Interaction
{
    [TestFixture]
    public class FSM_API_Interaction_MoveInstance_Tests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void MoveInstance_PreservesStateContextAndChangesSchedulingGroup()
        {
            const string fsmName = "RendererLOD";
            const string nearGroup = "LOD0";
            const string farGroup = "LOD3";

            BuildCompatibleDefinition(fsmName, nearGroup, processRate: 1);
            BuildCompatibleDefinition(fsmName, farGroup, processRate: 3);

            var context = new MoveTestContext();
            var handle = FSM_API.Create.CreateInstance(fsmName, context, nearGroup);

            FSM_API.Interaction.Update(nearGroup);

            Assert.That(handle.CurrentState, Is.EqualTo("Ready"));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(handle.ProcessingGroup, Is.EqualTo(nearGroup));

            FSM_API.Interaction.MoveInstance(handle, farGroup);

            // Structural changes are intentionally deferred until the safe update boundary.
            FSM_API.Internal.ProcessDeferredModifications();

            Assert.That(handle.Context, Is.SameAs(context));
            Assert.That(handle.CurrentState, Is.EqualTo("Ready"));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(handle.ProcessingGroup, Is.EqualTo(farGroup));
            Assert.That(FSM_API.Interaction.GetInstances(fsmName, nearGroup), Is.Empty);
            Assert.That(FSM_API.Interaction.GetInstances(fsmName, farGroup), Has.Count.EqualTo(1));
        }

        [Test]
        public void MoveInstance_ToMissingGroupLeavesInstanceInSourceGroup()
        {
            const string fsmName = "RendererLOD";
            const string sourceGroup = "LOD0";
            const string missingGroup = "LOD9";

            BuildCompatibleDefinition(fsmName, sourceGroup, processRate: 1);
            var handle = FSM_API.Create.CreateInstance(
                fsmName,
                new MoveTestContext(),
                sourceGroup);

            FSM_API.Interaction.MoveInstance(handle, missingGroup);

            FSM_API.Internal.ProcessDeferredModifications();

            Assert.That(handle.ProcessingGroup, Is.EqualTo(sourceGroup));
            Assert.That(FSM_API.Interaction.GetInstances(fsmName, sourceGroup), Has.Count.EqualTo(1));
        }

        [Test]
        public void DestroyInstance_UsesCurrentProcessingGroupAfterMove()
        {
            const string fsmName = "RendererLOD";
            const string sourceGroup = "LOD0";
            const string targetGroup = "LOD1";

            BuildCompatibleDefinition(fsmName, sourceGroup, processRate: 1);
            BuildCompatibleDefinition(fsmName, targetGroup, processRate: 2);

            var handle = FSM_API.Create.CreateInstance(
                fsmName,
                new MoveTestContext(),
                sourceGroup);

            FSM_API.Interaction.MoveInstance(handle, targetGroup);
            FSM_API.Internal.ProcessDeferredModifications();

            FSM_API.Interaction.DestroyInstance(handle);

            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0));
            Assert.That(handle.Context.IsValid, Is.False);
        }

        private static void BuildCompatibleDefinition(string name, string group, int processRate)
        {
            FSM_API.Create.CreateFiniteStateMachine(
                    name,
                    processRate: processRate,
                    processingGroup: group)
                .State("Ready", null, null, null)
                .WithInitialState("Ready")
                .BuildDefinition();
        }

        private sealed class MoveTestContext : IStateContext
        {
            public string Name { get; set; } = "MoveTest";
            public bool IsValid { get; set; } = true;
        }
    }
}
