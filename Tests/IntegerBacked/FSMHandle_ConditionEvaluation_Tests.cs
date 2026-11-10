using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Tests the integer-backed handle's manual condition-evaluation surface.
    /// </summary>
    [TestFixture]
    public sealed class FSMHandle_ConditionEvaluation_Tests
    {
        [Test]
        public void EvaluateConditions_UpdatesCurrentStateID()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => true);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(handle.CurrentStateID, Is.EqualTo(20));
        }

        [Test]
        public void EvaluateConditions_WhenFalse_PreservesCurrentStateID()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => false);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void EvaluateConditions_DoesNotRunUpdateAction()
        {
            var updated = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, _ => updated = true, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ => true);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(updated, Is.False);
        }

        [Test]
        public void EvaluateConditions_EntersTargetState()
        {
            var entered = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, _ => entered = true, null, null));
            fsm.AddTransition(10, 20, _ => true);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(entered, Is.True);
            Assert.That(handle.HasEnteredCurrentState, Is.True);
        }

        [Test]
        public void EvaluateConditions_PrioritizesAnyStateTransition()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddAnyStateTransition(30, _ => true);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(handle.CurrentStateID, Is.EqualTo(30));
        }

        [Test]
        public void EvaluateConditions_WhenContextIsInvalid_DoesNotEvaluateOrChangeLifecycle()
        {
            var conditionEvaluated = false;
            var exited = false;
            var entered = false;
            var context = new TestContext { IsValid = false };
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, _ => exited = true));
            fsm.AddState(new IntegerFSMState(20, _ => entered = true, null, null));
            fsm.AddTransition(10, 20, _ =>
            {
                conditionEvaluated = true;
                return true;
            });
            var handle = new IntegerFSMHandle(fsm, context);

            handle.EvaluateConditions();

            Assert.That(conditionEvaluated, Is.False);
            Assert.That(exited, Is.False);
            Assert.That(entered, Is.False);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
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