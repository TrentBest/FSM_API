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
            Assert.That(handle.HasEnteredCurrentState, Is.True);
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
    }
}
