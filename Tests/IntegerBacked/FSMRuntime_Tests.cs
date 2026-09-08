using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMRuntime_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void CreateInstance_InitializesTheHandleBeforeReturningIt()
        {
            var entered = 0;
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, _ => entered++, null, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);

            var handle = runtime.CreateInstance(7, new TestContext());

            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(entered, Is.EqualTo(1));
        }

        [Test]
        public void CreateInstance_AssignsUniqueInstanceIDs()
        {
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);

            var first = runtime.CreateInstance(7, new TestContext());
            var second = runtime.CreateInstance(7, new TestContext());

            Assert.That(second.Id, Is.EqualTo(first.Id + 1));
            Assert.That(runtime.GetHandleCount(0), Is.EqualTo(2));
        }

        [Test]
        public void UpdateAll_AdvancesInstancesAcrossProcessingGroups()
        {
            var firstUpdates = 0;
            var secondUpdates = 0;
            // This test exercises UpdateAll scheduling across groups, so both
            // definitions must be eligible for automatic processing.
            var first = new IntegerFSM(1, processRate: -1);
            first.AddState(new IntegerFSMState(10, null, _ => firstUpdates++, null));
            var second = new IntegerFSM(2, processRate: -1);
            second.AddState(new IntegerFSMState(20, null, _ => secondUpdates++, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(first, 100);
            runtime.Register(second, 200);
            runtime.CreateInstance(1, new TestContext());
            runtime.CreateInstance(2, new TestContext());

            runtime.UpdateAll();

            Assert.That(firstUpdates, Is.EqualTo(1));
            Assert.That(secondUpdates, Is.EqualTo(1));
        }

        [Test]
        public void UpdateAll_SkipsInvalidContexts()
        {
            var updates = 0;
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, null, _ => updates++, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);
            var context = new TestContext { IsValid = false };
            runtime.CreateInstance(7, context);

            runtime.UpdateAll();

            Assert.That(updates, Is.EqualTo(0));
        }

        [Test]
        public void RemoveInstance_RemovesOnlyTheSpecifiedHandle()
        {
            var fsm = new IntegerFSM(7);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);
            var first = runtime.CreateInstance(7, new TestContext());
            runtime.CreateInstance(7, new TestContext());

            Assert.That(runtime.RemoveInstance(first), Is.True);
            Assert.That(runtime.GetHandleCount(0), Is.EqualTo(1));
            Assert.That(runtime.RemoveInstance(first), Is.False);
        }

        [Test]
        public void UpdateAll_RespectsPositiveProcessRate()
        {
            var updates = 0;
            var fsm = new IntegerFSM(7, processRate: 3);
            fsm.AddState(new IntegerFSMState(10, null, _ => updates++, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);
            runtime.CreateInstance(7, new TestContext());

            for (var i = 0; i < 10; i++)
            {
                runtime.UpdateAll();
            }

            Assert.That(updates, Is.EqualTo(3));
        }

        [Test]
        public void UpdateAll_DoesNotProcessManualRate()
        {
            var updates = 0;
            var fsm = new IntegerFSM(7, processRate: 0);
            fsm.AddState(new IntegerFSMState(10, null, _ => updates++, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);
            runtime.CreateInstance(7, new TestContext());

            runtime.UpdateAll();
            runtime.UpdateAll();

            Assert.That(updates, Is.EqualTo(0));
        }

        [Test]
        public void UpdateAll_ProcessRateIsSharedByAllInstancesOfDefinition()
        {
            var updates = 0;
            var fsm = new IntegerFSM(7, processRate: 2);
            fsm.AddState(new IntegerFSMState(10, null, _ => updates++, null));
            var runtime = new IntegerFSMRuntime();
            runtime.Register(fsm);
            runtime.CreateInstance(7, new TestContext());
            runtime.CreateInstance(7, new TestContext());

            runtime.UpdateAll();
            Assert.That(updates, Is.EqualTo(0));

            runtime.UpdateAll();
            Assert.That(updates, Is.EqualTo(2));

            runtime.UpdateAll();
            Assert.That(updates, Is.EqualTo(2));

            runtime.UpdateAll();
            Assert.That(updates, Is.EqualTo(4));
        }
    }
}