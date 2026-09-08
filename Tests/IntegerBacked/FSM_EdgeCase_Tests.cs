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
