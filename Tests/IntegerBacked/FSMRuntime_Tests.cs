using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace FSM_API_Tests.IntegerBacked
{
    [TestFixture]
    public class FSMRuntime_Tests
    {
        [Test]
        public void Register_PreservesIntegerDefinitionIdentity()
        {
            var runtime = new IntegerFSMRuntime();
            var fsm = CreateBasicFSM(7);

            runtime.Register(fsm, 3);

            Assert.That(runtime.Contains(7), Is.True);
            Assert.That(runtime.GetDefinition(7), Is.SameAs(fsm));
            Assert.That(fsm.ProcessingGroupID, Is.EqualTo(3));
        }

        [Test]
        public void Register_ReplacesDefinitionWithSameIntegerIdentity()
        {
            var runtime = new IntegerFSMRuntime();
            var first = CreateBasicFSM(7);
            var replacement = CreateBasicFSM(7);

            runtime.Register(first, 1);
            runtime.Register(replacement, 2);

            Assert.That(runtime.GetDefinition(7), Is.SameAs(replacement));
            Assert.That(replacement.ProcessingGroupID, Is.EqualTo(2));
        }

        [Test]
        public void CreateInstance_RequiresRegisteredDefinition()
        {
            var runtime = new IntegerFSMRuntime();

            Assert.Throws<KeyNotFoundException>(() =>
                runtime.CreateInstance(99, new TestContext()));
        }

        [Test]
        public void CreateInstance_AssignsUniqueIntegerHandleIDs()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateBasicFSM(7));

            var first = runtime.CreateInstance(7, new TestContext());
            var second = runtime.CreateInstance(7, new TestContext());

            Assert.That(first, Is.TypeOf<IntegerFSMHandle>());
            Assert.That(first.Id, Is.Not.EqualTo(second.Id));
            Assert.That(first.FSM_ID, Is.EqualTo(7));
        }

        [Test]
        public void CreateInstance_StartsAtDefinitionInitialState()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateBasicFSM(7));

            var handle = runtime.CreateInstance(7, new TestContext());

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
        }

        [Test]
        public void Update_UpdatesOnlyMatchingProcessingGroup()
        {
            var runtime = new IntegerFSMRuntime();
            var first = CreateBasicFSM(7);
            var second = CreateBasicFSM(8);
            first.AddTransition(10, 20, _ => true);
            second.AddTransition(10, 20, _ => true);
            runtime.Register(first, 1);
            runtime.Register(second, 2);
            var firstHandle = runtime.CreateInstance(7, new TestContext());
            var secondHandle = runtime.CreateInstance(8, new TestContext());

            runtime.Update(1);

            Assert.That(firstHandle.CurrentStateID, Is.EqualTo(20));
            Assert.That(secondHandle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void Update_SkipsInvalidContext()
        {
            var runtime = new IntegerFSMRuntime();
            var fsm = CreateBasicFSM(7);
            fsm.AddTransition(10, 20, _ => true);
            runtime.Register(fsm, 1);
            var context = new TestContext { IsValid = false };
            var handle = runtime.CreateInstance(7, context);

            runtime.Update(1);

            Assert.That(handle.CurrentStateID, Is.EqualTo(10));
        }

        [Test]
        public void GetHandleCount_ReportsMatchingProcessingGroup()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateBasicFSM(7), 1);
            runtime.Register(CreateBasicFSM(8), 2);
            runtime.CreateInstance(7, new TestContext());
            runtime.CreateInstance(7, new TestContext());
            runtime.CreateInstance(8, new TestContext());

            Assert.That(runtime.GetHandleCount(1), Is.EqualTo(2));
            Assert.That(runtime.GetHandleCount(2), Is.EqualTo(1));
            Assert.That(runtime.GetHandleCount(99), Is.Zero);
        }

        private static IntegerFSM CreateBasicFSM(int id)
        {
            var fsm = new IntegerFSM(id);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
            fsm.AddState(new IntegerFSMState(20, null, null, null));
            return fsm;
        }

        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; } = "Test";
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }
    }
}
