using System;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Edge-case coverage for the integer-backed FSM definition.
    /// These tests deliberately probe boundary behavior before higher-level translation is introduced.
    /// </summary>
    [TestFixture]
    public sealed class FSM_EdgeCase_Tests
    {
        [Test]
        public void AddTransition_AllowsMissingSourceState()
        {
            var fsm = CreateFSMWithStates(10, 20);

            Assert.DoesNotThrow(() =>
                fsm.AddTransition(999, 20, _ => true));

            Assert.That(fsm.HasTransition(999, 20), Is.True);
        }

        [Test]
        public void AddTransition_AllowsMissingTargetState()
        {
            var fsm = CreateFSMWithStates(10, 20);

            Assert.DoesNotThrow(() =>
                fsm.AddTransition(10, 999, _ => true));

            Assert.That(fsm.HasTransition(10, 999), Is.True);
        }

        [Test]
        public void Step_RecoversInvalidCurrentStateToInitialState()
        {
            var entered = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, _ => entered = true, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.RemoveState(20);

            var result = fsm.Step(20, new TestContext());

            Assert.That(result, Is.EqualTo(10));
            Assert.That(entered, Is.True);
        }

        [Test]
        public void Step_WithInvalidCurrentAndMissingInitial_DoesNotThrow()
        {
            var fsm = new IntegerFSM(1);

            Assert.DoesNotThrow(() =>
                Assert.That(fsm.Step(999, new TestContext()), Is.EqualTo(IntegerFSM.AnyStateIdentifier)));
        }

        [Test]
        public void Step_IgnoresTransitionToMissingTargetState()
        {
            var fsm = CreateFSMWithStates(10, 20);
            fsm.AddTransition(10, 999, _ => true);

            var context = new TestContext();
            var result = fsm.Step(10, context);

            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void AddAnyStateTransition_ReplacesMatchingTarget()
        {
            var fsm = CreateFSMWithStates(10, 20);
            var firstCalled = false;
            var secondCalled = false;

            fsm.AddAnyStateTransition(20, _ =>
            {
                firstCalled = true;
                return true;
            });
            fsm.AddAnyStateTransition(20, _ =>
            {
                secondCalled = true;
                return true;
            });

            fsm.Step(10, new TestContext());

            Assert.That(firstCalled, Is.False);
            Assert.That(secondCalled, Is.True);
        }

        [Test]
        public void ForceTransition_AllowsMissingSourceState()
        {
            var fsm = CreateFSMWithStates(10, 20);
            var entered = false;
            fsm.AddState(new IntegerFSMState(20, _ => entered = true, null, null));

            Assert.DoesNotThrow(() =>
                fsm.ForceTransition(999, 20, new TestContext()));

            Assert.That(entered, Is.True);
        }

        [Test]
        public void ForceTransition_WhenExitThrows_StillEntersTargetState()
        {
            var entered = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, _ =>
                throw new InvalidOperationException("Exit failed.")));
            fsm.AddState(new IntegerFSMState(20, _ => entered = true, null, null));

            Assert.DoesNotThrow(() =>
                fsm.ForceTransition(10, 20, new TestContext()));

            Assert.That(entered, Is.True);
        }

        [Test]
        public void ForceTransition_WhenEnterThrows_PropagatesException()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, _ =>
                throw new InvalidOperationException("Enter failed."), null, null));

            Assert.Throws<InvalidOperationException>(() =>
                fsm.ForceTransition(10, 20, new TestContext()));
        }

        [Test]
        public void Handle_TransitionTo_WhenEnterThrows_PropagatesOriginalException()
        {
            var failure = new InvalidOperationException("Enter failed.");
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, _ => throw failure, null, null));
            var handle = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle(
                fsm, new TestContext());

            var thrown = Assert.Throws<InvalidOperationException>(() => handle.TransitionTo(20));

            Assert.That(thrown, Is.SameAs(failure));
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void Handle_EvaluateConditionsWithSameStateTransition_MarksStateEntered()
        {
            var exitCalls = 0;
            var enterCalls = 0;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, _ => enterCalls++, null, _ => exitCalls++));
            fsm.AddTransition(10, 10, _ => true);
            var handle = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle(
                fsm, new TestContext());

            handle.EvaluateConditions();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(exitCalls, Is.EqualTo(1));
            Assert.That(enterCalls, Is.EqualTo(1));
        }

        [Test]
        public void Handle_EvaluateConditionsWithoutTransition_PreservesEntryState()
        {
            var fsm = CreateFSMWithStates(10, 20);
            var handle = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle(
                fsm, new TestContext());

            Assert.That(handle.HasEnteredCurrentState, Is.False);

            handle.EvaluateConditions();

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
        }

        [Test]
        public void Step_WhenUpdateThrows_ContinuesToEvaluateTransitions()
        {
            var guardEvaluated = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, _ => throw new InvalidOperationException("Update failed."), null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddTransition(10, 20, _ =>
            {
                guardEvaluated = true;
                return true;
            });

            Assert.That(fsm.Step(10, new TestContext()), Is.EqualTo(20));
            Assert.That(guardEvaluated, Is.True);
        }

        [Test]
        public void Step_WhenExitThrows_ContinuesToLaterTransitionCandidate()
        {
            var exitCalls = 0;
            var laterGuardEvaluated = false;
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, _ =>
            {
                exitCalls++;
                if (exitCalls == 1)
                {
                    throw new InvalidOperationException("Exit failed.");
                }
            }));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            fsm.AddState(new IntegerFSMState(30, null, null, null));
            fsm.AddTransition(10, 20, _ => true);
            fsm.AddTransition(10, 30, _ =>
            {
                laterGuardEvaluated = true;
                return true;
            });

            Assert.That(fsm.Step(10, new TestContext()), Is.EqualTo(30));
            Assert.That(laterGuardEvaluated, Is.True);
            Assert.That(exitCalls, Is.EqualTo(2));
        }

        private static IntegerFSM CreateFSMWithStates(int firstID, int secondID)
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(firstID, null, null, null));
            fsm.AddState(new IntegerFSMState(secondID, null, null, null));
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
