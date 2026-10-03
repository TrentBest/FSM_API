using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Additional lifecycle coverage for integer-backed runtime definitions and instances.
    /// </summary>
    [TestFixture]
    public sealed class FSMRuntime_LifecycleEdgeCase_Tests
    {
        [Test]
        public void Unregister_RemovesAllInstancesForDefinition()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateFSM(7), 10);
            runtime.CreateInstance(7, new TestContext());
            runtime.CreateInstance(7, new TestContext());

            var removed = runtime.Unregister(7);

            Assert.That(removed, Is.True);
            Assert.That(runtime.Contains(7), Is.False);
            Assert.That(runtime.GetHandleCount(10), Is.Zero);
        }

        [Test]
        public void Register_ReplacementDoesNotRemoveUnrelatedDefinitionInstances()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateFSM(1), 10);
            runtime.Register(CreateFSM(2), 10);
            runtime.CreateInstance(1, new TestContext());
            runtime.CreateInstance(2, new TestContext());

            runtime.Register(CreateFSM(1), 20);

            Assert.That(runtime.GetHandleCount(10), Is.EqualTo(1));
            Assert.That(runtime.GetDefinition(1).ProcessingGroupID, Is.EqualTo(20));
        }

        [Test]
        public void CreateInstance_UsesNextHandleIDAfterRemoval()
        {
            var runtime = new IntegerFSMRuntime();
            runtime.Register(CreateFSM(1));
            var first = runtime.CreateInstance(1, new TestContext());
            runtime.RemoveInstance(first);
            var second = runtime.CreateInstance(1, new TestContext());

            Assert.That(second.Id, Is.GreaterThan(first.Id));
        }

        [Test]
        public void Update_EmptyRuntimeIsSafe()
        {
            var runtime = new IntegerFSMRuntime();

            Assert.DoesNotThrow(() => runtime.Update(10));
        }

        private static IntegerFSM CreateFSM(int id)
        {
            var fsm = new IntegerFSM(id);
            fsm.AddState(new IntegerFSMState(10, null, null, null));
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
