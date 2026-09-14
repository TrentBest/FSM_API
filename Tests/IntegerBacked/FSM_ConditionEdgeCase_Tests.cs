using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Additional contract coverage for integer-backed manual transition evaluation.
    /// </summary>
    [TestFixture]
    public sealed class FSM_ConditionEdgeCase_Tests
    {
        [Test]
        public void EvaluateConditions_UnknownCurrentState_ReturnsSameIdentity()
        {
            var fsm = CreateFSM();

            var result = fsm.EvaluateConditions(999, new TestContext());

            Assert.That(result, Is.EqualTo(999));
        }

        [Test]
        public void EvaluateConditions_FalseAnyStateFallsThroughToRegularTransition()
        {
            var fsm = CreateFSM();
            fsm.AddAnyStateTransition(20, _ => false);
            fsm.AddTransition(10, 30, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(30));
        }

        [Test]
        public void EvaluateConditions_IgnoresMissingAnyStateTarget()
        {
            var fsm = CreateFSM();
            fsm.AddAnyStateTransition(999, _ => true);
            fsm.AddTransition(10, 20, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(20));
        }

        [Test]
        public void EvaluateConditions_IgnoresMissingRegularTarget()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 999, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void EvaluateConditions_TrueTransitionExecutesExitAndEnter()
        {
            var lifecycle = string.Empty;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, _ => lifecycle += "X"));
            fsm.AddState(new IntegerFSMState(20, _ => lifecycle += "E", null, null));
            fsm.AddTransition(10, 20, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(20));
            Assert.That(lifecycle, Is.EqualTo("XE"));
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
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }
    }
}
