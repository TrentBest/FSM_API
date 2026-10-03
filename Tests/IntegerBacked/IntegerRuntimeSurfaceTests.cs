using NUnit.Framework;
using TheSingularityWorkshop.FSM_API.IntegerBacked;
using IntegerFsm = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFsmState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFsmApi = TheSingularityWorkshop.FSM_API.IntegerBacked.FsmApi;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public sealed class IntegerRuntimeSurfaceTests
    {
        private sealed class Context : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "IntegerRuntimeSurfaceTests";
            public int Context_ID => 21;
            public bool HasEnteredCurrentState { get; set; }
        }

        [Test]
        public void PublicFacade_BuildsRegistersCreatesAndUpdatesIntegerFsm()
        {
            const int fsmID = 21001;
            const int processingGroupID = 7;
            var entered = 0;
            var updated = 0;
            var exited = 0;

            try
            {
                var definition = IntegerFsmApi.Create.CreateFiniteStateMachine(fsmID, -1, processingGroupID)
                    .State(0, _ => entered++, _ => updated++, _ => exited++)
                    .State(1)
                    .Transition(0, 1, _ => true)
                    .BuildDefinition();

                IntegerFsmApi.Create.RegisterDefinition(definition);
                var handle = IntegerFsmApi.Create.CreateInstance(fsmID, new Context());

                Assert.That(handle.CurrentStateID, Is.EqualTo(0));
                Assert.That(entered, Is.EqualTo(1));

                IntegerFsmApi.Interaction.Update(processingGroupID);

                Assert.That(updated, Is.EqualTo(1));
                Assert.That(exited, Is.EqualTo(1));
                Assert.That(handle.CurrentStateID, Is.EqualTo(1));
                Assert.That(IntegerFsmApi.Interaction.Contains(fsmID), Is.True);
            }
            finally
            {
                IntegerFsmApi.Interaction.Unregister(fsmID);
            }
        }

        [Test]
        public void RuntimeProcessRate_UsesIntegerProcessingIdentityWithoutStringLookup()
        {
            const int fsmID = 21003;
            const int processingGroupID = 9;
            var updates = 0;

            try
            {
                var definition = IntegerFsmApi.Create.CreateFiniteStateMachine(fsmID, 3, processingGroupID)
                    .State(0, onUpdate: _ => updates++)
                    .BuildDefinition();

                IntegerFsmApi.Create.RegisterDefinition(definition);
                IntegerFsmApi.Create.CreateInstance(fsmID, new Context());

                IntegerFsmApi.Interaction.Update(processingGroupID);
                Assert.That(updates, Is.EqualTo(0));

                IntegerFsmApi.Interaction.Update(processingGroupID);
                Assert.That(updates, Is.EqualTo(0));

                IntegerFsmApi.Interaction.Update(processingGroupID);
                Assert.That(updates, Is.EqualTo(1));
            }
            finally
            {
                IntegerFsmApi.Interaction.Unregister(fsmID);
            }
        }

        [Test]
        public void PublicFacade_SeparatesIntegerIdentityFromStringBackedApi()
        {
            const int fsmID = 21002;
            const int stateID = 42;

            var definition = IntegerFsmApi.Create.CreateFiniteStateMachine(fsmID)
                .State(stateID)
                .WithInitialState(stateID)
                .BuildDefinition();

            Assert.That(definition.FSM_ID, Is.EqualTo(fsmID));
            Assert.That(definition.InitialStateID, Is.EqualTo(stateID));
            Assert.That(definition.GetState(stateID).StateID, Is.EqualTo(stateID));
            Assert.That(definition.GetState(stateID).ToString(), Does.Contain("42"));
        }
    }
}
