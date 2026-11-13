using System;
using NUnit.Framework;
using ShortApi = global::FsmApi.FsmApi;
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
            FsmApi.Internal.ResetAPI(true);
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
        public void ShortNamespaceErrorFacade_ExposesIntegerRuntimeDiagnosticSnapshots()
        {
            var instanceCounts = ShortApi.Error.GetIntegerInstanceErrorCounts();
            var definitionCounts = ShortApi.Error.GetIntegerDefinitionErrorCounts();

            Assert.That(instanceCounts, Is.Not.Null);
            Assert.That(definitionCounts, Is.Not.Null);

            // Returned dictionaries are snapshots, not mutable views over runtime state.
            instanceCounts.Clear();
            definitionCounts.Clear();

            Assert.That(ShortApi.Error.GetIntegerInstanceErrorCounts(), Is.Empty);
            Assert.That(ShortApi.Error.GetIntegerDefinitionErrorCounts(), Is.Empty);
            Assert.DoesNotThrow(() => ShortApi.Error.ResetIntegerInstanceErrorCount(null));
            Assert.DoesNotThrow(() => ShortApi.Error.ResetIntegerDefinitionErrorCount(733));
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
            TheSingularityWorkshop.FSM_API.FsmApi.Error.InternalApiErrorEventHandler handler =
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
