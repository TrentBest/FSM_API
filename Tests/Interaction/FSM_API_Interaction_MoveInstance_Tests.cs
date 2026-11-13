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
            FsmApi.Internal.ResetAPI(true);
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
            var handle = FsmApi.Create.CreateInstance(fsmName, context, nearGroup);

            FsmApi.Interaction.Update(nearGroup);

            Assert.That(handle.CurrentState, Is.EqualTo("Ready"));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(handle.ProcessingGroup, Is.EqualTo(nearGroup));

            FsmApi.Interaction.MoveInstance(handle, farGroup);

            // Structural changes are intentionally deferred until the safe update boundary.
            FsmApi.Internal.ProcessDeferredModifications();

            Assert.That(handle.Context, Is.SameAs(context));
            Assert.That(handle.CurrentState, Is.EqualTo("Ready"));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(handle.ProcessingGroup, Is.EqualTo(farGroup));
            Assert.That(FsmApi.Interaction.GetInstances(fsmName, nearGroup), Is.Empty);
            Assert.That(FsmApi.Interaction.GetInstances(fsmName, farGroup), Has.Count.EqualTo(1));
        }

        [Test]
        public void MoveInstance_ToMissingGroupLeavesInstanceInSourceGroup()
        {
            const string fsmName = "RendererLOD";
            const string sourceGroup = "LOD0";
            const string missingGroup = "LOD9";

            BuildCompatibleDefinition(fsmName, sourceGroup, processRate: 1);
            var handle = FsmApi.Create.CreateInstance(
                fsmName,
                new MoveTestContext(),
                sourceGroup);

            FsmApi.Interaction.MoveInstance(handle, missingGroup);

            FsmApi.Internal.ProcessDeferredModifications();

            Assert.That(handle.ProcessingGroup, Is.EqualTo(sourceGroup));
            Assert.That(FsmApi.Interaction.GetInstances(fsmName, sourceGroup), Has.Count.EqualTo(1));
        }

        [Test]
        public void DestroyInstance_UsesCurrentProcessingGroupAfterMove()
        {
            const string fsmName = "RendererLOD";
            const string sourceGroup = "LOD0";
            const string targetGroup = "LOD1";

            BuildCompatibleDefinition(fsmName, sourceGroup, processRate: 1);
            BuildCompatibleDefinition(fsmName, targetGroup, processRate: 2);

            var handle = FsmApi.Create.CreateInstance(
                fsmName,
                new MoveTestContext(),
                sourceGroup);

            FsmApi.Interaction.MoveInstance(handle, targetGroup);
            FsmApi.Internal.ProcessDeferredModifications();

            FsmApi.Interaction.DestroyInstance(handle);

            Assert.That(FsmApi.Internal.TotalFsmHandleCount, Is.EqualTo(0));
            Assert.That(handle.Context.IsValid, Is.True,
                "Destroying a handle must not mutate the caller-owned context.");
            Assert.That(handle.IsDestroyed, Is.True,
                "Handle teardown state must be independent from context validity.");
        }

        private static void BuildCompatibleDefinition(string name, string group, int processRate)
        {
            FsmApi.Create.CreateFiniteStateMachine(
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
