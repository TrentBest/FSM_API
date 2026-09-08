using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace FSM_API_Tests.IntegerBacked
{
    [TestFixture]
    public class FSMHandle_Tests
    {
        [Test]
        public void Constructor_PreservesIntegerIdentity()
        {
            var fsm = CreateBasicFSM();
            var context = new TestContext();
            var handle = new IntegerFSMHandle(fsm, context, 42);

            Assert.That(handle.Id, Is.EqualTo(42));
            Assert.That(handle.FSM_ID, Is.EqualTo(7));
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.Context, Is.SameAs(context));
        }

        [Test]
        public void Constructor_RejectsNullDefinition()
        {
            Assert.That(
                () => new IntegerFSMHandle(null, new TestContext()),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_RejectsNullContext()
        {
            Assert.That(
                () => new IntegerFSMHandle(CreateBasicFSM(), null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Update_FollowsIntegerTransition()
        {
            var fsm = CreateBasicFSM();
            fsm.AddTransition(10, 20, _ => true);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(20));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
        }

        [Test]
        public void Update_WhenConditionFalse_RemainsInCurrentState()
        {
            var fsm = CreateBasicFSM();
            fsm.AddTransition(10, 20, _ => false);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void EvaluateConditions_FollowsIntegerTransitionWithoutUpdate()
        {
            var fsm = CreateBasicFSM();
            var events = "";
            fsm.GetState(10).SetOnUpdate(_ => events += "U");
            fsm.GetState(10).SetOnExit(_ => events += "X");
            fsm.GetState(20).SetOnEnter(_ => events += "E");
            fsm.AddTransition(10, 20, _ => true);
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(handle.CurrentStateID, Is.EqualTo(20));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(events, Is.EqualTo("XE"));
        }

        [Test]
        public void EvaluateConditions_WhenFalseRemainsInCurrentState()
        {
            var fsm = CreateBasicFSM();
            var handle = new IntegerFSMHandle(fsm, new TestContext());
            fsm.AddTransition(10, 20, _ => false);

            handle.EvaluateConditions();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
        }

        [Test]
        public void TransitionTo_UsesIntegerIdentity()
        {
            var fsm = CreateBasicFSM();
            var handle = new IntegerFSMHandle(fsm, new TestContext());

            handle.TransitionTo(20);

            Assert.That(handle.CurrentStateID, Is.EqualTo(20));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
        }

        [Test]
        public void TransitionTo_RejectsMissingTargetState()
        {
            var handle = new IntegerFSMHandle(CreateBasicFSM(), new TestContext());

            Assert.That(
                () => handle.TransitionTo(999),
                Throws.InvalidOperationException);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void ResetFSMInstance_ReturnsToInitialIntegerState()
        {
            var fsm = CreateBasicFSM();
            var handle = new IntegerFSMHandle(fsm, new TestContext());
            handle.TransitionTo(20);

            handle.ResetFSMInstance();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void IsValid_ReflectsContextValidity()
        {
            var context = new TestContext { IsValid = true };
            var handle = new IntegerFSMHandle(CreateBasicFSM(), context);

            Assert.That(handle.IsValid, Is.True);

            context.IsValid = false;

            Assert.That(handle.IsValid, Is.False);
        }

        private static IntegerFSM CreateBasicFSM()
        {
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
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
