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


        [Test]
        public void RemoveInstance_RunsExitExactlyOnceAndPreventsFurtherUpdates()
        {
            var exits = 0;
            var updates = 0;
            var runtime = new IntegerFSMRuntime();
            var fsm = new IntegerFSM(8, -1);
            fsm.AddState(new IntegerFSMState(10, null, _ => updates++, _ => exits++));
            runtime.Register(fsm);
            var handle = runtime.CreateInstance(8, new TestContext());

            Assert.That(runtime.RemoveInstance(handle), Is.True);
            Assert.That(runtime.RemoveInstance(handle), Is.False);
            handle.Update();

            Assert.That(exits, Is.EqualTo(1));
            Assert.That(updates, Is.Zero);
            Assert.That(handle.IsDestroyed, Is.True);
            Assert.That(runtime.GetHandleCount(0), Is.Zero);
        }

        [Test]
        public void Unregister_RunsExitForEveryRemovedInstance()
        {
            var exits = 0;
            var runtime = new IntegerFSMRuntime();
            var fsm = new IntegerFSM(9, -1);
            fsm.AddState(new IntegerFSMState(10, null, null, _ => exits++));
            runtime.Register(fsm);
            runtime.CreateInstance(9, new TestContext());
            runtime.CreateInstance(9, new TestContext());

            Assert.That(runtime.Unregister(9), Is.True);

            Assert.That(exits, Is.EqualTo(2));
            Assert.That(runtime.GetHandleCount(0), Is.Zero);
        }

        [Test]
        public void UpdateAll_IsolatesRepeatedInstanceFailuresAndDefersTeardown()
        {
            var previousInstanceThreshold = FSM_API.Error.InstanceErrorThreshold;
            var previousDefinitionThreshold = FSM_API.Error.DefinitionErrorThreshold;
            var exits = 0;
            try
            {
                FSM_API.Error.InstanceErrorThreshold = 2;
                FSM_API.Error.DefinitionErrorThreshold = 10;
                var runtime = new IntegerFSMRuntime();
                var fsm = new IntegerFSM(10, -1);
                fsm.AddState(new IntegerFSMState(10, null, null, null));
                fsm.AddState(new IntegerFSMState(11,
                    _ => throw new System.InvalidOperationException("enter failed"), null, _ => exits++));
                fsm.AddTransition(10, 11, _ => true);
                runtime.Register(fsm);
                var handle = runtime.CreateInstance(10, new TestContext());

                // First tick transitions to state 11. Its Enter callback is attempted on the
                // following tick and fails; the runtime records rather than leaks that exception.
                Assert.DoesNotThrow(() => runtime.UpdateAll());
                Assert.That(handle.IsDestroyed, Is.False);
                Assert.That(runtime.GetHandleCount(0), Is.EqualTo(1));

                Assert.DoesNotThrow(() => runtime.UpdateAll());
                Assert.That(handle.IsDestroyed, Is.False);

                Assert.DoesNotThrow(() => runtime.UpdateAll());

                Assert.That(handle.IsDestroyed, Is.True);
                Assert.That(exits, Is.EqualTo(1));
                Assert.That(runtime.GetHandleCount(0), Is.Zero);
            }
            finally
            {
                FSM_API.Error.InstanceErrorThreshold = previousInstanceThreshold;
                FSM_API.Error.DefinitionErrorThreshold = previousDefinitionThreshold;
            }
        }

        [Test]
        public void CreateInstance_WhenInitialEnterUnregistersDefinition_DestroysInProgressHandle()
        {
            var exits = 0;
            var runtime = new IntegerFSMRuntime();
            var fsm = new IntegerFSM(11, -1);
            fsm.AddState(new IntegerFSMState(10,
                _ => runtime.Unregister(11), null, _ => exits++));
            runtime.Register(fsm);

            var handle = runtime.CreateInstance(11, new TestContext());

            Assert.That(runtime.Contains(11), Is.False);
            Assert.That(runtime.GetHandleCount(0), Is.Zero);
            Assert.That(handle.IsDestroyed, Is.True);
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(exits, Is.EqualTo(1));
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
