using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Edge-case coverage for integer-backed runtime orchestration.
    /// </summary>
    [TestFixture]
    public sealed class FSMRuntime_EdgeCase_Tests
    {
        [Test]
        public void Update_OnlyUpdatesMatchingProcessingGroup()
        {
            var runtime = new IntegerFSMRuntime();
            var groupOneUpdated = false;
            var groupTwoUpdated = false;

            var groupOne = CreateFSM(1, _ => groupOneUpdated = true);
            var groupTwo = CreateFSM(2, _ => groupTwoUpdated = true);

            runtime.Register(groupOne, 10);
            runtime.Register(groupTwo, 20);
            runtime.CreateInstance(1, new TestContext());
            runtime.CreateInstance(2, new TestContext());

            runtime.Update(10);

            Assert.That(groupOneUpdated, Is.True);
            Assert.That(groupTwoUpdated, Is.False);
        }

        [Test]
        public void Update_SkipsInvalidContextWithoutRemovingHandle()
        {
            var runtime = new IntegerFSMRuntime();
            var updated = false;
            var fsm = CreateFSM(1, _ => updated = true);
            runtime.Register(fsm, 10);
            var context = new TestContext { IsValid = false };
            runtime.CreateInstance(1, context);

            runtime.Update(10);

            Assert.That(updated, Is.False);
            Assert.That(runtime.GetHandleCount(10), Is.EqualTo(1));
        }

        [Test]
        public void CreateInstance_AssignsDistinctHandleIDs()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateFSM(1, null));

            var first = runtime.CreateInstance(1, new TestContext());
            var second = runtime.CreateInstance(1, new TestContext());

            Assert.That(first.Id, Is.Not.EqualTo(second.Id));
        }

        [Test]
        public void GetHandleCount_IsScopedToProcessingGroup()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateFSM(1, null), 10);
            runtime.Register(CreateFSM(2, null), 20);
            runtime.CreateInstance(1, new TestContext());
            runtime.CreateInstance(1, new TestContext());
            runtime.CreateInstance(2, new TestContext());

            Assert.That(runtime.GetHandleCount(10), Is.EqualTo(2));
            Assert.That(runtime.GetHandleCount(20), Is.EqualTo(1));
            Assert.That(runtime.GetHandleCount(30), Is.Zero);
        }

        private static IntegerFSM CreateFSM(int id, System.Action<IStateContext> update)
        {
            var fsm = new IntegerFSM(id);
            fsm.AddState(new IntegerFSMState(10, null, update, null));
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
