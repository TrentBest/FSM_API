using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;

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
        public void EvaluateConditions_WhenGuardThrows_ContinuesToLaterCandidate()
        {
            var laterGuardEvaluated = false;
            var fsm = CreateFSM();
            fsm.AddAnyStateTransition(20, _ => throw new System.InvalidOperationException("Guard failed."));
            fsm.AddAnyStateTransition(30, _ =>
            {
                laterGuardEvaluated = true;
                return true;
            });
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            Assert.DoesNotThrow(() => handle.EvaluateConditions());

            Assert.That(laterGuardEvaluated, Is.True);
            Assert.That(handle.CurrentStateID, Is.EqualTo(30));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
        }

        [Test]
        public void EvaluateConditions_WhenGuardRemovesItsTarget_ContinuesToLaterCandidate()
        {
            var laterGuardEvaluated = false;
            var fsm = CreateFSM();
            fsm.AddAnyStateTransition(20, _ =>
            {
                fsm.RemoveState(20);
                return true;
            });
            fsm.AddAnyStateTransition(30, _ =>
            {
                laterGuardEvaluated = true;
                return true;
            });
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(laterGuardEvaluated, Is.True);
            Assert.That(handle.CurrentStateID, Is.EqualTo(30));
        }

        [Test]
        public void EvaluateConditions_WhenContextIsInvalid_DoesNotEvaluateOrChangeLifecycle()
        {
            var conditionEvaluated = false;
            var exited = false;
            var entered = false;
            var context = new TestContext();
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, _ => exited = true));
            fsm.AddState(new IntegerFSMState(20, _ => entered = true, null, null));
            fsm.AddTransition(10, 20, _ =>
            {
                conditionEvaluated = true;
                return true;
            });
            var handle = new IntegerFSMHandle(fsm, context);
            // Construction establishes the handle's initial validity; invalidation
            // after construction must suspend manual condition evaluation.
            context.IsValid = false;

            handle.EvaluateConditions();

            Assert.That(conditionEvaluated, Is.False);
            Assert.That(exited, Is.False);
            Assert.That(entered, Is.False);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
        }

        [Test]
        public void EvaluateConditions_WhenExitCallbackUnregistersDefinition_DoesNotInvokeExitTwice()
        {
            var exits = 0;
            var targetEnters = 0;
            var runtime = new IntegerFSMRuntime();
            var fsm = new IntegerFSM(31);
            fsm.AddState(new IntegerFSMState(10, null, null, _ =>
            {
                exits++;
                runtime.Unregister(31);
            }));
            fsm.AddState(new IntegerFSMState(20, _ => targetEnters++, null, null));
            fsm.AddTransition(10, 20, _ => true);
            runtime.Register(fsm);
            var handle = runtime.CreateInstance(31, new TestContext());

            Assert.DoesNotThrow(() => handle.EvaluateConditions());

            Assert.That(handle.IsDestroyed, Is.True);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(exits, Is.EqualTo(1));
            Assert.That(targetEnters, Is.Zero);
            Assert.That(runtime.GetHandleCount(0), Is.Zero);
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