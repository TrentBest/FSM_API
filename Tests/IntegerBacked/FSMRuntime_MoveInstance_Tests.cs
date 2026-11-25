using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public sealed class FSMRuntime_MoveInstance_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; } = "MoveInstance";
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void MoveInstance_PreservesStateAndContextWhenTargetContainsCurrentState()
        {
            var runtime = new IntegerFSMRuntime();
            var source = CreateDefinition(51, 7, null);
            var target = CreateDefinition(52, 8, null);
            runtime.Register(source, 1);
            runtime.Register(target, 2);
            var context = new TestContext();
            var handle = runtime.CreateInstance(51, context);

            runtime.MoveInstance(handle, 52);

            Assert.That(handle.Definition, Is.SameAs(target));
            Assert.That(handle.FSM_ID, Is.EqualTo(52));
            Assert.That(handle.CurrentStateID, Is.EqualTo(7));
            Assert.That(handle.Context, Is.SameAs(context));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(runtime.GetHandleCount(1), Is.Zero);
            Assert.That(runtime.GetHandleCount(2), Is.EqualTo(1));
        }

        [Test]
        public void MoveInstance_RejectsTargetWithoutCurrentStateAndLeavesHandleUntouched()
        {
            var runtime = new IntegerFSMRuntime();
            var source = CreateDefinition(53, 7, null);
            var target = CreateDefinition(54, 8, null);
            runtime.Register(source, 1);
            runtime.Register(target, 2);
            var handle = runtime.CreateInstance(53, new TestContext());

            Assert.That(
                () => runtime.MoveInstance(handle, 54),
                Throws.TypeOf<InvalidOperationException>());

            Assert.That(handle.Definition, Is.SameAs(source));
            Assert.That(handle.FSM_ID, Is.EqualTo(53));
            Assert.That(runtime.GetHandleCount(1), Is.EqualTo(1));
            Assert.That(runtime.GetHandleCount(2), Is.Zero);
        }

        [Test]
        public void MoveInstance_RejectsUnknownTargetWithoutChangingSource()
        {
            var runtime = new IntegerFSMRuntime();
            var source = CreateDefinition(55, 7, null);
            runtime.Register(source, 1);
            var handle = runtime.CreateInstance(55, new TestContext());

            Assert.That(
                () => runtime.MoveInstance(handle, 56),
                Throws.TypeOf<System.Collections.Generic.KeyNotFoundException>());

            Assert.That(handle.Definition, Is.SameAs(source));
            Assert.That(runtime.GetHandleCount(1), Is.EqualTo(1));
        }

        [Test]
        public void MoveRequestedDuringUpdate_CommitsAfterAllDefinitionsFinishCurrentTick()
        {
            var trace = new List<string>();
            var runtime = new IntegerFSMRuntime();
            IntegerFSMHandle handle = null;

            var source = CreateDefinition(57, 7, _ =>
            {
                trace.Add("source.update");
                runtime.MoveInstance(handle, 58);
            });
            var target = CreateDefinition(58, 7, _ => trace.Add("target.update"));
            runtime.Register(source, 3);
            runtime.Register(target, 3);
            handle = runtime.CreateInstance(57, new TestContext());

            runtime.Update(3);

            Assert.That(handle.FSM_ID, Is.EqualTo(58));
            Assert.That(trace, Is.EqualTo(new[] { "source.update" }),
                "The moved handle must not run a second time in the destination definition during the same tick.");

            runtime.Update(3);

            Assert.That(trace, Is.EqualTo(new[] { "source.update", "target.update" }));
        }

        [Test]
        public void MoveRequestedDuringUpdate_UsesLastTargetRequestedForHandle()
        {
            var runtime = new IntegerFSMRuntime();
            IntegerFSMHandle handle = null;
            var source = CreateDefinition(59, 7, _ =>
            {
                runtime.MoveInstance(handle, 60);
                runtime.MoveInstance(handle, 61);
            });
            var firstTarget = CreateDefinition(60, 7, null);
            var lastTarget = CreateDefinition(61, 7, null);
            runtime.Register(source, 4);
            runtime.Register(firstTarget, 4);
            runtime.Register(lastTarget, 4);
            handle = runtime.CreateInstance(59, new TestContext());

            runtime.Update(4);

            Assert.That(handle.Definition, Is.SameAs(lastTarget));
        }

        private static IntegerFSM CreateDefinition(int id, int stateID, Action<IStateContext> onUpdate)
        {
            var definition = new IntegerFSM(id, -1);
            definition.AddState(new IntegerFSMState(stateID, null, onUpdate, null));
            return definition;
        }
    }
}
