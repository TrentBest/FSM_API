using System.Linq;

using NUnit.Framework;

using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFSMTransition = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMTransition;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSM_Collection_Tests
    {
        [Test]
        public void HasState_ReturnsFalseForMissingState()
        {
            var fsm = CreateFSM();

            Assert.That(fsm.HasState(999), Is.False);
        }

        [Test]
        public void GetState_ReturnsNullForMissingState()
        {
            var fsm = CreateFSM();

            Assert.That(fsm.GetState(999), Is.Null);
        }

        [Test]
        public void GetAllStates_ReturnsAllRegisteredStates()
        {
            var fsm = CreateFSM();

            var states = fsm.GetAllStates();

            Assert.That(states.Count, Is.EqualTo(2));
            Assert.That(states.Any(state => state.StateID == 10), Is.True);
            Assert.That(states.Any(state => state.StateID == 20), Is.True);
        }

        [Test]
        public void HasTransition_ReturnsFalseForMissingTransition()
        {
            var fsm = CreateFSM();

            Assert.That(fsm.HasTransition(10, 99), Is.False);
        }

        [Test]
        public void GetAllTransitions_ReturnsRegularAndAnyStateTransitions()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => false);
            fsm.AddAnyStateTransition(10, _ => false);

            var transitions = fsm.GetAllTransitions();

            Assert.That(transitions.Count, Is.EqualTo(2));
            Assert.That(transitions.Any(t => t.FromID == 10 && t.ToID == 20), Is.True);
            Assert.That(transitions.Any(t => t.FromID == IntegerFSM.AnyStateIdentifier && t.ToID == 10), Is.True);
        }

        [Test]
        public void GetAllTransitions_DoesNotExposeInternalCollection()
        {
            var fsm = CreateFSM();
            fsm.AddTransition(10, 20, _ => false);

            var transitions = fsm.GetAllTransitions();
            var originalCount = transitions.Count;

            Assert.That(originalCount, Is.EqualTo(1));
            Assert.That(transitions, Is.Not.SameAs(fsm.GetAllTransitions()));
        }

        private static IntegerFSM CreateFSM()
        {
            var fsm = new IntegerFSM(1);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            return fsm;
        }
    }
}
