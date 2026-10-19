using System;
using System.Collections.Generic;
using System.Reflection;

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


        [Test]
        public void CreateInstance_RejectsExhaustedHandleIDRangeWithoutWrapping()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateFSM(1));
            typeof(IntegerFSMRuntime)
                .GetField("_nextHandleID", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(runtime, int.MaxValue);

            Assert.Throws<InvalidOperationException>(() =>
                runtime.CreateInstance(1, new TestContext()));
            Assert.That(runtime.GetInstances(1), Is.Empty);
        }

        [Test]
        public void Register_RejectsMaximumFSMIDBeforeCapacityCalculation()
        {
            var runtime = new IntegerFSMRuntime();
            var fsm = CreateFSM(int.MaxValue);

            Assert.Throws<ArgumentOutOfRangeException>(() => runtime.Register(fsm));
            Assert.That(runtime.Contains(int.MaxValue), Is.False);
        }

        [Test]
        public void AddState_RejectsMaximumStateIDBeforeCapacityCalculation()
        {
            var fsm = new IntegerFSM(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                fsm.AddState(new IntegerFSMState(int.MaxValue, null, null, null)));
            Assert.That(fsm.HasState(int.MaxValue), Is.False);
        }

        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }

        private static IntegerFSM CreateFSM(int id)
        {
            var fsm = new IntegerFSM(id);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            return fsm;
        }
    }
}
