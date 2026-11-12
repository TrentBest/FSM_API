using NUnit.Framework;
using PublicApi = FSM_API.FsmApi;
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
            FSM_API.Internal.ResetAPI();
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
