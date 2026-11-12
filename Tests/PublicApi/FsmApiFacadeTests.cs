using System;
using NUnit.Framework;
using PublicApi = global::FSM_API.FsmApi;
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

            PublicApi.Create.CreateFiniteStateMachine(definitionName, -1, group)
                .State("Active", null, context => ((Context)context).Updates++, null)
                .WithInitialState("Active")
                .BuildDefinition();

            var context = new Context();
            var handle = PublicApi.Create.CreateInstance(definitionName, context, group);

            PublicApi.Interact.Update(group);

            Assert.That(handle, Is.Not.Null);
            Assert.That(context.IsValid, Is.True);
            Assert.That(context.Updates, Is.EqualTo(1));
        }


        [Test]
        public void ShortNamespaceFacade_CreatesAndUpdatesIntegerBackedInstance()
        {
            const int definitionID = 731;
            const int processingGroupID = 19;

            var definition = PublicApi.Create.CreateFiniteStateMachine(definitionID, -1, processingGroupID)
                .State(0, onUpdate: context => ((Context)context).Updates++)
                .WithInitialState(0)
                .BuildDefinition();
            PublicApi.Create.RegisterDefinition(definition);

            var context = new Context();
            var handle = PublicApi.Create.CreateInstance(definitionID, context);

            PublicApi.Interact.Update(processingGroupID);

            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.IsValid, Is.True);
            Assert.That(context.Updates, Is.EqualTo(1));
            Assert.That(PublicApi.Interact.Contains(definitionID), Is.True);
            Assert.That(PublicApi.Interact.GetFSMDefinition(definitionID), Is.SameAs(definition));
            Assert.That(PublicApi.Interact.GetInstances(definitionID).Count, Is.EqualTo(1));

            Assert.That(PublicApi.Interact.DestroyInstance(handle), Is.True);
            Assert.That(PublicApi.Interact.GetInstances(definitionID).Count, Is.Zero);
            Assert.That(PublicApi.Interact.Unregister(definitionID), Is.True);
        }

        [Test]
        public void ShortNamespaceErrorFacade_ExposesIntegerRuntimeDiagnostics()
        {
            const int definitionID = 733;
            const int processingGroupID = 21;

            var definition = PublicApi.Create.CreateFiniteStateMachine(definitionID, -1, processingGroupID)
                .State(0, onUpdate: context => throw new InvalidOperationException("expected diagnostic test failure"))
                .WithInitialState(0)
                .BuildDefinition();
            PublicApi.Create.RegisterDefinition(definition);

            var handle = PublicApi.Create.CreateInstance(definitionID, new Context());
            try
            {
                PublicApi.Interact.Update(processingGroupID);

                var counts = PublicApi.Error.GetIntegerInstanceErrorCounts();
                Assert.That(counts.TryGetValue(handle, out var count), Is.True);
                Assert.That(count, Is.EqualTo(1));

                PublicApi.Error.ResetIntegerInstanceErrorCount(handle);
                Assert.That(PublicApi.Error.GetIntegerInstanceErrorCounts().ContainsKey(handle), Is.False);
            }
            finally
            {
                PublicApi.Interact.DestroyInstance(handle);
                PublicApi.Interact.Unregister(definitionID);
            }
        }

        [Test]
        public void ShortNamespaceTransferFacade_CompilesAndDecompilesDefinitions()
        {
            const string definitionName = "PublicFacadeTransferTest";
            const string group = "PublicFacadeTransferGroup";

            PublicApi.Create.CreateFiniteStateMachine(definitionName, -1, group)
                .State("Active", null, null, null)
                .WithInitialState("Active")
                .BuildDefinition();

            var source = PublicApi.Interact.GetFSMDefinition(definitionName, group);
            var compiled = PublicApi.Transfer.ToInteger(source, 732, 20);

            Assert.That(compiled.Definition.FSM_ID, Is.EqualTo(732));
            Assert.That(compiled.StateNameToID["Active"], Is.EqualTo(0));

            var reconstructed = PublicApi.Transfer.ToString(
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

            PublicApi.Error.OnInternalApiError += handler;
            try
            {
                PublicApi.Error.Report("facade diagnostic");
            }
            finally
            {
                PublicApi.Error.OnInternalApiError -= handler;
            }

            Assert.That(observedMessage, Is.EqualTo("facade diagnostic"));
        }
    }
}
