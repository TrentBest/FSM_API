using System;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFSMTransition = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMTransition;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Contract coverage for integer-backed FSM collection mutation and validation behavior.
    /// </summary>
    [TestFixture]
    public class FSM_MutationAndValidation_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void Constructor_UsesAnyStateAsInitialWhenEmpty()
        {
            var fsm = new IntegerFSM(42);

            Assert.That(fsm.InitialStateID, Is.EqualTo(IntegerFSM.AnyStateIdentifier));
            Assert.That(fsm.HasState(IntegerFSM.AnyStateIdentifier), Is.False);
        }

        [Test]
        public void AddState_DoesNotChangeInitialStateWhenAddingLaterState()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));

            Assert.That(fsm.InitialStateID, Is.EqualTo(10));
        }

        [Test]
        public void GetAllStates_ReturnsSnapshotRatherThanBackingCollection()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));

            var states = fsm.GetAllStates();
            fsm.AddState(new IntegerFSMState(20, null, null, null));

            Assert.That(states, Has.Count.EqualTo(1));
            Assert.That(fsm.GetAllStates(), Has.Count.EqualTo(2));
        }

        [Test]
        public void AddAnyStateTransition_ReplacesOnlyMatchingDestination()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddState(new IntegerFSMState(30, null, null, null));
            fsm.AddAnyStateTransition(20, _ => true);
            fsm.AddAnyStateTransition(30, _ => true);

            fsm.AddAnyStateTransition(20, _ => false);

            Assert.That(fsm.GetAllTransitions(), Has.Count.EqualTo(2));
            Assert.That(fsm.Step(10, new TestContext()), Is.EqualTo(30));
        }

        [Test]
        public void GetAllTransitions_ReturnsAnyStateAndRegularTransitions()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddState(new IntegerFSMState(30, null, null, null));
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddAnyStateTransition(30, _ => true);

            var transitions = fsm.GetAllTransitions();

            Assert.That(transitions, Has.Count.EqualTo(2));
            Assert.That(transitions, Has.Some.Matches<IntegerFSMTransition>(t =>
                t.FromID == 10 && t.ToID == 20));
            Assert.That(transitions, Has.Some.Matches<IntegerFSMTransition>(t =>
                t.FromID == IntegerFSM.AnyStateIdentifier && t.ToID == 30));
        }

        [Test]
        public void RemoveTransition_WhenNoMatchExistsLeavesOtherTransitionsUntouched()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ => true);

            fsm.RemoveTransition(10, 99);

            Assert.That(fsm.HasTransition(10, 20), Is.True);
            Assert.That(fsm.GetAllTransitions(), Has.Count.EqualTo(1));
        }

        [Test]
        public void RemoveState_WhenStateDoesNotExistLeavesFSMUntouched()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ => true);

            fsm.RemoveState(99);

            Assert.That(fsm.GetAllStates(), Has.Count.EqualTo(2));
            Assert.That(fsm.InitialStateID, Is.EqualTo(10));
            Assert.That(fsm.HasTransition(10, 20), Is.True);
        }

        [Test]
        public void Step_SkipsTransitionWhoseDestinationStateWasRemoved()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ => true);
            fsm.RemoveState(20);

            Assert.That(fsm.Step(10, new TestContext()), Is.EqualTo(10));
        }

        [Test]
        public void EvaluateConditions_SkipsTransitionWhoseDestinationStateWasRemoved()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ => true);
            fsm.RemoveState(20);

            Assert.That(fsm.EvaluateConditions(10, new TestContext()), Is.EqualTo(10));
        }

        [Test]
        public void EvaluateConditions_TakesAtMostOneTransition()
        {
            var enteredSecond = false;
            var enteredThird = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, _ => enteredSecond = true, null, null));
            fsm.AddState(new IntegerFSMState(30, _ => enteredThird = true, null, null));
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddTransition(20, 30, _ => true);

            var result = fsm.EvaluateConditions(10, new TestContext());

            Assert.That(result, Is.EqualTo(20));
            Assert.That(enteredSecond, Is.True);
            Assert.That(enteredThird, Is.False);
        }

        [Test]
        public void Step_TakesAtMostOneTransition()
        {
            var enteredSecond = false;
            var enteredThird = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, _ => enteredSecond = true, null, null));
            fsm.AddState(new IntegerFSMState(30, _ => enteredThird = true, null, null));
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddTransition(20, 30, _ => true);

            var result = fsm.Step(10, new TestContext());

            Assert.That(result, Is.EqualTo(20));
            Assert.That(enteredSecond, Is.True);
            Assert.That(enteredThird, Is.False);
        }

        [Test]
        public void ForceTransition_WithUnknownSourceOnlyEntersValidTarget()
        {
            var entered = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(20, _ => entered = true, null, null));

            fsm.ForceTransition(99, 20, new TestContext());

            Assert.That(entered, Is.True);
        }

        [Test]
        public void ForceTransition_DoesNotExitSourceWhenSourceIsUnknown()
        {
            var exited = false;
            var entered = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(20, _ => entered = true, null, null));

            fsm.ForceTransition(99, 20, new TestContext());

            Assert.That(exited, Is.False);
            Assert.That(entered, Is.True);
        }
    }
}
