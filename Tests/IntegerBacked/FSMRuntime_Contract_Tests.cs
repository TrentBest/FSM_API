using System.Collections.Generic;

using NUnit.Framework;

using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMRuntime_Contract_Tests
    {
        [Test]
        public void Contains_ReturnsFalseForUnregisteredDefinition()
        {
            var runtime = new IntegerFSMRuntime();

            Assert.That(runtime.Contains(404), Is.False);
        }

        [Test]
        public void GetDefinition_ReturnsNullForUnregisteredDefinition()
        {
            var runtime = new IntegerFSMRuntime();

            Assert.That(runtime.GetDefinition(404), Is.Null);
        }

        [Test]
        public void Register_AssignsProcessingGroupToDefinition()
        {
            var runtime = new IntegerFSMRuntime();
            var fsm = CreateFSM(1);

            runtime.Register(fsm, 7);

            Assert.That(fsm.ProcessingGroupID, Is.EqualTo(7));
        }

        [Test]
        public void Register_MakesDefinitionDiscoverableByID()
        {
            var runtime = new IntegerFSMRuntime();
            var fsm = CreateFSM(12);

            runtime.Register(fsm);

            Assert.That(runtime.Contains(12), Is.True);
            Assert.That(runtime.GetDefinition(12), Is.SameAs(fsm));
        }

        [Test]
        public void Unregister_ReturnsFalseForUnknownDefinition()
        {
            var runtime = new IntegerFSMRuntime();

            Assert.That(runtime.Unregister(404), Is.False);
        }

        [Test]
        public void GetHandleCount_ReturnsZeroForEmptyGroup()
        {
            var runtime = new IntegerFSMRuntime();

            Assert.That(runtime.GetHandleCount(7), Is.Zero);
        }

        private static IntegerFSM CreateFSM(int id)
        {
            var fsm = new IntegerFSM(id);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            return fsm;
        }
    }
}
