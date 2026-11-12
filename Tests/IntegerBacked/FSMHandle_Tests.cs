using System.Reflection;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle;
using IntegerFSMHandleInt = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMHandle_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void Constructor_SetsInitialStateButDoesNotEnterIt()
        {
            var entered = false;
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, _ => entered = true, null, null));
            var handle = new IntegerFSMHandle(fsm, new TestContext(), 42);

            Assert.That(handle.Id, Is.EqualTo(42));
            Assert.That(handle.FSM_ID, Is.EqualTo(7));
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(entered, Is.False);
        }

        [Test]
        public void Constructor_InitializesCallerOwnedContextAsValid()
        {
            var context = new TestContext { IsValid = false };
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, null, null, null));

            var handle = new IntegerFSMHandle(fsm, context, 42);

            Assert.That(context.IsValid, Is.True);
            Assert.That(handle.Context, Is.SameAs(context));
            Assert.That(handle.IsValid, Is.True);
        }

        [Test]
        public void IntegerRuntime_ReturnsExplicitIntegerHandle()
        {
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(0, null, null, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);

            var handle = runtime.CreateInstance(7, new TestContext());

            Assert.That(handle, Is.TypeOf<IntegerFSMHandleInt>());
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
        }

        [Test]
        public void IntegerHandle_HasNoStringCurrentStateProperty()
        {
            var property = typeof(IntegerFSMHandleInt).GetProperty("CurrentState", BindingFlags.Instance | BindingFlags.Public);

            Assert.That(property, Is.Null);
            Assert.That(typeof(IntegerFSMHandleInt).GetProperty("CurrentStateID"), Is.Not.Null);
        }

        [Test]
        public void Initialize_EntersInitialStateAndMarksHandleEntered()
        {
            var entered = 0;
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, _ => entered++, null, null));
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.Initialize();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(entered, Is.EqualTo(1));
        }

        [Test]
        public void Initialize_IsIdempotent()
        {
            var entered = 0;
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, _ => entered++, null, null));
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.Initialize();
            handle.Initialize();

            Assert.That(entered, Is.EqualTo(1));
        }

        [Test]
        public void Initialize_DoesNotPreventLaterTransition()
        {
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ => true);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.Initialize();
            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(20));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
        }

        [Test]
        public void IsValid_TracksCurrentContextValidity()
        {
            var context = new TestContext { IsValid = true };
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            var handle = new IntegerFSMHandle(fsm, context);

            Assert.That(handle.IsValid, Is.True);

            context.IsValid = false;

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void EvaluateConditions_InvalidatingGuardStopsBeforeExit()
        {
            var guardCount = 0;
            var exitCount = 0;
            var suspendOnce = true;

            var fsm = new IntegerFSM(27);
            fsm.AddState(new IntegerFSMState(10, null, null, _ => exitCount++));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, context =>
            {
                guardCount++;
                if (suspendOnce)
                {
                    suspendOnce = false;
                    context.IsValid = false;
                    return true;
                }
                return false;
            });

            var context = new TestContext();
            var handle = new IntegerFSMHandle(fsm, context);

            handle.EvaluateConditions();

            Assert.That(context.IsValid, Is.False);
            Assert.That(guardCount, Is.EqualTo(1));
            Assert.That(exitCount, Is.Zero);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));

            context.IsValid = true;
            handle.EvaluateConditions();

            Assert.That(guardCount, Is.EqualTo(2));
            Assert.That(exitCount, Is.Zero);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void ResetFSMInstance_WhenInitialStateIsMissingMatchesStringBackedLifecycle()
        {
            var fsm = new IntegerFSM(31);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            var handle = new IntegerFSMHandle(fsm, new TestContext());
            fsm.RemoveState(10);

            handle.ResetFSMInstance();

            // The string-backed reset attempts its ordinary forced transition. A missing
            // initial target does not replace the handle's current identity with a sentinel.
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
        }

        [Test]
        public void TransitionTo_MissingTargetMatchesStringBackedLifecycle()
        {
            var exitCount = 0;
            var fsm = new IntegerFSM(30);
            fsm.AddState(new IntegerFSMState(10, null, null, _ => exitCount++));
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.TransitionTo(404);

            // String-backed TryForceTransition runs the source Exit before discovering
            // that the target is absent. The current identity remains unchanged, but
            // the state is no longer considered entered after its Exit callback ran.
            Assert.That(exitCount, Is.EqualTo(1));
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
        }

        [Test]
        public void ForcedTransition_StopsAfterOnExitInvalidatesContext()
        {
            var targetEnterCount = 0;
            var suspendOnce = true;

            var fsm = new IntegerFSM(28);
            fsm.AddState(new IntegerFSMState(10, null, null, context =>
            {
                if (suspendOnce)
                {
                    suspendOnce = false;
                    context.IsValid = false;
                }
            }));
            fsm.AddState(new IntegerFSMState(20, _ => targetEnterCount++, null, null));

            var context = new TestContext();
            var handle = new IntegerFSMHandle(fsm, context);

            handle.TransitionTo(20);

            Assert.That(context.IsValid, Is.False);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(targetEnterCount, Is.Zero);

            context.IsValid = true;
            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(targetEnterCount, Is.Zero);
        }


        [Test]
        public void ForcedTransition_TargetEnterMaySuspendAfterCommittingTarget()
        {
            var targetEnterCount = 0;
            var targetUpdateCount = 0;

            var fsm = new IntegerFSM(29);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, context =>
            {
                targetEnterCount++;
                context.IsValid = false;
            }, _ => targetUpdateCount++, null));

            var context = new TestContext();
            var handle = new IntegerFSMHandle(fsm, context);

            handle.TransitionTo(20);

            Assert.That(context.IsValid, Is.False);
            Assert.That(handle.CurrentStateID, Is.EqualTo(20));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(targetEnterCount, Is.EqualTo(1));

            context.IsValid = true;
            handle.Update();

            Assert.That(targetEnterCount, Is.EqualTo(1));
            Assert.That(targetUpdateCount, Is.EqualTo(1));
            Assert.That(handle.CurrentStateID, Is.EqualTo(20));
        }

    }
}
