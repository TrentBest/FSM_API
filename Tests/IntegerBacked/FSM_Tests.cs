using System;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSM_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void Constructor_PreservesIntegerIdentity()
        {
            var fsm = new IntegerFSM(42);

            Assert.That(fsm.FSM_ID, Is.EqualTo(42));
            Assert.That(fsm.ProcessRate, Is.Zero);
        }

        [Test]
        public void AddState_PreservesIntegerIdentityAndSetsFirstStateInitial()
        {
            var fsm = new IntegerFSM(1);
            var state = new IntegerFSMState(10);

            fsm.AddState(state);

            Assert.That(fsm.HasState(10), Is.True);
            Assert.That(fsm.GetState(10), Is.SameAs(state));
            Assert.That(fsm.InitialStateID, Is.EqualTo(10));
        }

        [Test]
        public void AddState_ReplacesExistingState()
        {
            var fsm = new IntegerFSM(1);
            var first = new IntegerFSMState(10);
            var replacement = new IntegerFSMState(10);

            fsm.AddState(first);
            fsm.AddState(replacement);

            Assert.That(fsm.GetState(10), Is.SameAs(replacement));
            Assert.That(fsm.GetAllStates(), Has.Count.EqualTo(1));
        }

        [Test]
        public void AddState_RejectsNull()
        {
            var fsm = new IntegerFSM(1);

            Assert.Throws<ArgumentNullException>(() => fsm.AddState(null));
        }

        [Test]
        public void AddTransition_PreservesIntegerEndpoints()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10));
            fsm.AddState(new IntegerFSMState(20));

            fsm.AddTransition(10, 20, _ => true);

            Assert.That(fsm.HasTransition(10, 20), Is.True);
            Assert.That(fsm.GetAllTransitions(), Has.Count.EqualTo(1));
        }

        [Test]
        public void Step_FollowsTrueRegularTransition()
        {
            var fsm = new IntegerFSM(1);
            var first = new IntegerFSMState(10);
            var second = new IntegerFSMState(20);
            fsm.AddState(first);
            fsm.AddState(second);
            fsm.AddTransition(10, 20, _ => true);
            var context = new TestContext();

            var result = fsm.Step(10, context);

            Assert.That(result, Is.EqualTo(20));
        }

        [Test]
        public void Step_RemainsInCurrentStateWhenConditionIsFalse()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10));
            fsm.AddState(new IntegerFSMState(20));
            fsm.AddTransition(10, 20, _ => false);

            Assert.That(fsm.Step(10, new TestContext()), Is.EqualTo(10));
        }

        [Test]
        public void Step_EvaluatesAnyStateBeforeRegularTransition()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10));
            fsm.AddState(new IntegerFSMState(20));
            fsm.AddState(new IntegerFSMState(30));
            fsm.AddAnyStateTransition(30, _ => true);
            fsm.AddTransition(10, 20, _ => true);

            Assert.That(fsm.Step(10, new TestContext()), Is.EqualTo(30));
        }

        [Test]
        public void EnterInitial_InvokesInitialStateEnter()
        {
            var entered = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10).SetOnEnter(_ => entered = true));

            fsm.EnterInitial(new TestContext());

            Assert.That(entered, Is.True);
        }

        [Test]
        public void ForceTransition_ExitsAndEntersStates()
        {
            var exited = false;
            var entered = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10).SetOnExit(_ => exited = true));
            fsm.AddState(new IntegerFSMState(20).SetOnEnter(_ => entered = true));

            fsm.ForceTransition(10, 20, new TestContext());

            Assert.That(exited, Is.True);
            Assert.That(entered, Is.True);
        }

        [Test]
        public void RemoveState_RemovesConnectedTransitions()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10));
            fsm.AddState(new IntegerFSMState(20));
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddAnyStateTransition(20, _ => true);

            fsm.RemoveState(20);

            Assert.That(fsm.HasState(20), Is.False);
            Assert.That(fsm.HasTransition(10, 20), Is.False);
            Assert.That(fsm.GetAllTransitions(), Is.Empty);
        }

        [Test]
        public void RemoveTransition_RemovesOnlyMatchingTransition()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10));
            fsm.AddState(new IntegerFSMState(20));
            fsm.AddState(new IntegerFSMState(30));
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddTransition(10, 30, _ => true);

            fsm.RemoveTransition(10, 20);

            Assert.That(fsm.HasTransition(10, 20), Is.False);
            Assert.That(fsm.HasTransition(10, 30), Is.True);
        }

        [Test]
        public void AddAnyStateTransition_UsesReservedAnyStateIdentity()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10));
            fsm.AddState(new IntegerFSMState(20));
            fsm.AddAnyStateTransition(20, _ => true);

            Assert.That(fsm.HasTransition(IntegerFSM.AnyStateIdentifier, 20), Is.True);
        }
    }
}
