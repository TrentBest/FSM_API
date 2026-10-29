using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public sealed class FsmApiFacadeTests
    {
        private sealed class Context : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "FsmApiFacadeTests";
            public int Context_ID => 321;
            public bool HasEnteredCurrentState { get; set; }
        }

        [Test]
        public void CreateFiniteStateMachine_OverloadsKeepStringAndIntegerBuildersDistinct()
        {
            var stringBuilder = FsmApi.Create.CreateFiniteStateMachine(
                "FsmApiFacadeTests.String", processRate: -1, processingGroup: "FsmApiFacadeTests");
            var integerBuilder = FsmApi.Create.CreateFiniteStateMachine(
                32101, processRate: -1, processingGroupID: 321);

            Assert.That(stringBuilder, Is.Not.Null);
            Assert.That(integerBuilder, Is.Not.Null);
            Assert.That(integerBuilder.BuildDefinition().FSM_ID, Is.EqualTo(32101));
        }

        [Test]
        public void IntegerFacade_CreatesAndUpdatesWithoutUsingStringRegistry()
        {
            const int fsmID = 32102;
            const int groupID = 322;
            var updates = 0;

            try
            {
                var definition = FsmApi.Create.CreateFiniteStateMachine(fsmID, -1, groupID)
                    .State(0, onUpdate: _ => updates++)
                    .BuildDefinition();

                FsmApi.Create.RegisterDefinition(definition);
                FsmApi.Create.CreateInstance(fsmID, new Context());
                FsmApi.Interaction.Update(groupID);

                Assert.That(updates, Is.EqualTo(1));
                Assert.That(FsmApi.Interaction.Contains(fsmID), Is.True);
            }
            finally
            {
                FsmApi.Interaction.Unregister(fsmID);
            }
        }
    }
}
