using System;
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

        [TestCase(IntegerFSM.MaximumSupportedArrayIndex + 1)]
        [TestCase(int.MaxValue)]
        public void AddState_RejectsIDsBeyondSupportedArrayBoundary(int stateID)
        {
            var fsm = new IntegerFSM(0);

            Assert.That(
                () => fsm.AddState(new IntegerFSMState(stateID, null, null, null)),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(IntegerFSM.MaximumSupportedArrayIndex + 1)]
        [TestCase(int.MaxValue)]
        public void Register_RejectsIDsBeyondSupportedArrayBoundary(int fsmID)
        {
            var runtime = new IntegerFSMRuntime();
            var fsm = CreateFSM(fsmID);

            Assert.That(() => runtime.Register(fsm), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AddState_PreservesSparseIDsWithinSupportedBoundary()
        {
            var fsm = new IntegerFSM(0);
            fsm.AddState(new IntegerFSMState(3, null, null, null));
            fsm.AddState(new IntegerFSMState(17, null, null, null));

            Assert.That(fsm.HasState(3), Is.True);
            Assert.That(fsm.HasState(17), Is.True);
            Assert.That(fsm.HasState(4), Is.False);
        }

        private static IntegerFSM CreateFSM(int id)
        {
            var fsm = new IntegerFSM(id);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            return fsm;
        }
    }
}
