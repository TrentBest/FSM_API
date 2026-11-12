using System;
using NUnit.Framework;
using ShortApi = global::FSM_API.FsmApi;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests.PublicApi
{
    [TestFixture]
    public sealed class FsmApiFacadeTests
    {
        private sealed class Context : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "FacadeTestContext";
            public int Updates { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void ShortNamespaceFacade_CreatesAndUpdatesStringBackedInstance()
        {
            const string definitionName = "PublicFacadeSmokeTest";
            const string group = "PublicFacadeGroup";

            ShortApi.Create.CreateFiniteStateMachine(definitionName, -1, group)
                .State("Active", null, context => ((Context)context).Updates++, null)
                .WithInitialState("Active")
                .BuildDefinition();

            var context = new Context();
            var handle = ShortApi.Create.CreateInstance(definitionName, context, group);

            ShortApi.Interact.Update(group);

            Assert.That(handle, Is.Not.Null);
            Assert.That(context.IsValid, Is.True);
            Assert.That(context.Updates, Is.EqualTo(1));
        }


        [Test]
        public void ShortNamespaceFacade_CreatesAndUpdatesIntegerBackedInstance()
        {
            const int definitionID = 731;
            const int processingGroupID = 19;

            var definition = ShortApi.Create.CreateFiniteStateMachine(definitionID, -1, processingGroupID)
                .State(0, onUpdate: context => ((Context)context).Updates++)
                .WithInitialState(0)
                .BuildDefinition();
            ShortApi.Create.RegisterDefinition(definition);

            var context = new Context();
            var handle = ShortApi.Create.CreateInstance(definitionID, context);

            ShortApi.Interact.Update(processingGroupID);

            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.IsValid, Is.True);
            Assert.That(context.Updates, Is.EqualTo(1));
            Assert.That(ShortApi.Interact.Contains(definitionID), Is.True);
            Assert.That(ShortApi.Interact.GetFSMDefinition(definitionID), Is.SameAs(definition));
            Assert.That(ShortApi.Interact.GetInstances(definitionID).Count, Is.EqualTo(1));

            Assert.That(ShortApi.Interact.DestroyInstance(handle), Is.True);
            Assert.That(ShortApi.Interact.GetInstances(definitionID).Count, Is.Zero);
            Assert.That(ShortApi.Interact.Unregister(definitionID), Is.True);
        }

        [Test]
        public void ShortNamespaceErrorFacade_ExposesIntegerRuntimeDiagnostics()
        {
            const int definitionID = 733;
            const int processingGroupID = 21;

            var definition = ShortApi.Create.CreateFiniteStateMachine(definitionID, -1, processingGroupID)
                .State(0, onUpdate: context => throw new InvalidOperationException("expected diagnostic test failure"))
                .WithInitialState(0)
                .BuildDefinition();
            ShortApi.Create.RegisterDefinition(definition);

            var handle = ShortApi.Create.CreateInstance(definitionID, new Context());
            var previousThreshold = ShortApi.Error.InstanceErrorThreshold;
            ShortApi.Error.InstanceErrorThreshold = 5;
            try
            {
                ShortApi.Interact.Update(processingGroupID);

                var counts = ShortApi.Error.GetIntegerInstanceErrorCounts();
                Assert.That(counts.TryGetValue(handle, out var count), Is.True);
                Assert.That(count, Is.EqualTo(1));

                ShortApi.Error.ResetIntegerInstanceErrorCount(handle);
                Assert.That(ShortApi.Error.GetIntegerInstanceErrorCounts().ContainsKey(handle), Is.False);
            }
            finally
            {
                ShortApi.Interact.DestroyInstance(handle);
                ShortApi.Interact.Unregister(definitionID);
                ShortApi.Error.InstanceErrorThreshold = previousThreshold;
            }
        }

        [Test]
        public void ShortNamespaceTransferFacade_CompilesAndDecompilesDefinitions()
        {
            const string definitionName = "PublicFacadeTransferTest";
            const string group = "PublicFacadeTransferGroup";

            ShortApi.Create.CreateFiniteStateMachine(definitionName, -1, group)
                .State("Active", null, null, null)
                .WithInitialState("Active")
                .BuildDefinition();

            var source = ShortApi.Interact.GetFSMDefinition(definitionName, group);
            var compiled = ShortApi.Transfer.ToInteger(source, 732, 20);

            Assert.That(compiled.Definition.FSM_ID, Is.EqualTo(732));
            Assert.That(compiled.StateNameToID["Active"], Is.EqualTo(0));

            var reconstructed = ShortApi.Transfer.ToString(
                compiled.Definition, "PublicFacadeTransferRoundTrip", group, compiled.StateIDToName);

            Assert.That(reconstructed.Name, Is.EqualTo("PublicFacadeTransferRoundTrip"));
            Assert.That(reconstructed.InitialState, Is.EqualTo("Active"));
        }

        [Test]
        public void ShortNamespaceErrorFacade_ForwardsDiagnosticNotifications()
        {
            string observedMessage = null;
            TheSingularityWorkshop.FSM_API.FSM_API.Error.InternalApiErrorEventHandler handler =
                (message, exception) => observedMessage = message;

            ShortApi.Error.OnInternalApiError += handler;
            try
            {
                ShortApi.Error.Report("facade diagnostic");
            }
            finally
            {
                ShortApi.Error.OnInternalApiError -= handler;
            }

            Assert.That(observedMessage, Is.EqualTo("facade diagnostic"));
        }
    }
}
