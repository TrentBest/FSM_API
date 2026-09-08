using NUnit.Framework;
using System;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Atomic tests for the integer-backed FSM state.
    /// </summary>
    [TestFixture]
    public class FSMState_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid => true;
        }

        [Test]
        public void Constructor_PreservesIntegerIdentity()
        {
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(42, null, null, null);

            Assert.That(state, Is.Not.Null);
            Assert.That(state.StateID, Is.EqualTo(42));
        }

        [Test]
        public void Constructor_AllowsZeroIdentity()
        {
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(0, null, null, null);

            Assert.That(state.StateID, Is.EqualTo(0));
        }

        [Test]
        public void Enter_InvokesAssignedAction()
        {
            bool invoked = false;
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(1, ctx => invoked = true, null, null);

            state.Enter(new TestContext());

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void Update_InvokesAssignedAction()
        {
            bool invoked = false;
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(1, null, ctx => invoked = true, null);

            state.Update(new TestContext());

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void Exit_InvokesAssignedAction()
        {
            bool invoked = false;
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(1, null, null, ctx => invoked = true);

            state.Exit(new TestContext());

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void NullActions_AreSafe()
        {
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(1, null, null, null);
            var context = new TestContext();

            Assert.DoesNotThrow(() =>
            {
                state.Enter(context);
                state.Update(context);
                state.Exit(context);
            });
        }

        [Test]
        public void Setters_CanReplaceActions()
        {
            int calls = 0;
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(1, null, null, null);

            state.SetOnEnter(ctx => calls++);
            state.SetOnUpdate(ctx => calls++);
            state.SetOnExit(ctx => calls++);

            var context = new TestContext();
            state.Enter(context);
            state.Update(context);
            state.Exit(context);

            Assert.That(calls, Is.EqualTo(3));
        }

        [Test]
        public void Setters_NullRestoreSafeDefaults()
        {
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(1, ctx => throw new InvalidOperationException(), null, null);

            state.SetOnEnter(null);

            Assert.DoesNotThrow(() => state.Enter(new TestContext()));
        }

        [Test]
        public void ToString_UsesIntegerIdentity()
        {
            var state = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(12345, null, null, null);

            Assert.That(state.ToString(), Is.EqualTo("FSMState: 12345"));
        }
    }
}
