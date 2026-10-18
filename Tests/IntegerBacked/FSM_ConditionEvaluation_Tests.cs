using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Tests the manual transition-evaluation path of the integer-backed FSM.
    /// </summary>
    [TestFixture]
    public sealed class FSM_ConditionEvaluation_Tests
    {
        [Test]
        public void EvaluateConditions_TakesTrueRegularTransition()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(20));
        }

        [Test]
        public void EvaluateConditions_WhenConditionFalse_RemainsInCurrentState()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => false);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void EvaluateConditions_PrioritizesAnyStateTransition()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddAnyStateTransition(30, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(30));
        }

        [Test]
        public void EvaluateConditions_TakesAtMostOneTransition()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddTransition(20, 30, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(20));
        }

        [Test]
        public void EvaluateConditions_DoesNotRunCurrentStateUpdate()
        {
            var updated = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, _ => updated = true, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ => true);

            fsm.EvaluateConditions(10, new TestContext());

            Assert.That(updated, Is.False);
        }

        private static IntegerFSM CreateFSM()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddState(new IntegerFSMState(30, null, null, null));
            return fsm;
        }

        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; } = "Test";
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }
    }
}
