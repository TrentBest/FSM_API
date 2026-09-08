using System.Reflection;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSM_ArrayBackedIdentity_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void StateStorage_IsAnArrayIndexedByStateID()
        {
            var field = typeof(IntegerFSM).GetField("_states", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);
            Assert.That(field.FieldType, Is.EqualTo(typeof(IntegerFSMState[])));
        }

        [Test]
        public void StateID_IsUsedAsDirectArrayIndex()
        {
            var fsm = new IntegerFSM(1);
            var zero = new IntegerFSMState(0, null, null, null);
            var two = new IntegerFSMState(2, null, null, null);

            fsm.AddState(zero);
            fsm.AddState(two);

            Assert.That(fsm.GetState(0), Is.SameAs(zero));
            Assert.That(fsm.GetState(2), Is.SameAs(two));
            Assert.That(fsm.GetState(1), Is.Null);
        }

        [Test]
        public void NegativeStateIDs_AreRejectedBecauseTheyCannotBeArrayIndexes()
        {
            var fsm = new IntegerFSM(1);

            Assert.That(
                () => fsm.AddState(new IntegerFSMState(-1, null, null, null)),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void IntegerHandle_TransitionsByComparingIntegerStateIDs()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(0, null, null, null));
            fsm.AddState(new IntegerFSMState(1, null, null, null));
            fsm.AddTransition(0, 1, _ => true);

            var handle = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt(fsm, new TestContext());
            handle.Initialize();
            handle.Update();

            Assert.That(handle.CurrentStateID == 1, Is.True);
        }
    }
}
