using NUnit.Framework;
using TheSingularityWorkshop.FSM_API.IntegerBacked;
using IntegerFsm = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFsmState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFsmHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt;
using IntegerFsmTimers = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMTimersInt;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public sealed class IntegerParityTests
    {
        private sealed class Context : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "IntegerParityTests";
            public int Context_ID => 23;
            public bool HasEnteredCurrentState { get; set; }
        }

        [Test]
        public void AnyStateTransition_TakesPriorityOverStateSpecificTransition()
        {
            var definition = new IntegerFsm(23001);
            definition.AddState(new IntegerFsmState(0, null, null, null));
            definition.AddState(new IntegerFsmState(1, null, null, null));
            definition.AddState(new IntegerFsmState(2, null, null, null));
            definition.AddTransition(0, 1, _ => true);
            definition.AddAnyStateTransition(2, _ => true);

            var result = definition.Step(0, new Context());

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void Handle_ForceTransitionAndReset_PreserveLifecycleSemantics()
        {
            var entered = 0;
            var exited = 0;
            var definition = new IntegerFsm(23002);
            definition.AddState(new IntegerFsmState(0, _ => entered++, null, _ => exited++));
            definition.AddState(new IntegerFsmState(1, _ => entered++, null, _ => exited++));

            var handle = new IntegerFsmHandle(definition, new Context());
            handle.Initialize();
            Assert.That(entered, Is.EqualTo(1));

            handle.TransitionTo(1);
            Assert.That(handle.CurrentStateID, Is.EqualTo(1));
            Assert.That(entered, Is.EqualTo(2));
            Assert.That(exited, Is.EqualTo(1));

            handle.ResetFSMInstance();
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(entered, Is.EqualTo(3));
            Assert.That(exited, Is.EqualTo(2));
        }


        [Test]
        public void Handle_NormalTransitionDefersTargetEntryUntilNextUpdate()
        {
            var trace = new System.Collections.Generic.List<string>();
            var definition = new IntegerFsm(23006);
            definition.AddState(new IntegerFsmState(0,
                _ => trace.Add("enter:A"),
                _ => trace.Add("update:A"),
                _ => trace.Add("exit:A")));
            definition.AddState(new IntegerFsmState(1,
                _ => trace.Add("enter:B"),
                _ => trace.Add("update:B"),
                _ => trace.Add("exit:B")));
            definition.AddTransition(0, 1, _ => { trace.Add("guard:A:B"); return true; });

            var handle = new IntegerFsmHandle(definition, new Context());
            handle.Initialize();
            trace.Clear();

            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(1));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(trace, Is.EqualTo(new[] { "update:A", "guard:A:B", "exit:A" }));

            trace.Clear();
            handle.Update();

            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "enter:B", "update:B" }));
        }

        [Test]
        public void Handle_InvalidCurrentStateRecovery_EntersInitialStateOnlyOnce()
        {
            var trace = new System.Collections.Generic.List<string>();
            var definition = new IntegerFsm(23007);
            definition.AddState(new IntegerFsmState(0,
                _ => trace.Add("enter:initial"),
                _ => trace.Add("update:initial"),
                _ => trace.Add("exit:initial")));
            definition.AddState(new IntegerFsmState(1, null, null, null));

            var handle = new IntegerFsmHandle(definition, new Context());
            handle.Initialize();
            handle.CurrentStateID = 99; // Simulate a stale ID after definition mutation.
            handle.HasEnteredCurrentState = true;
            trace.Clear();

            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "enter:initial" }));

            trace.Clear();
            handle.Update();
            Assert.That(trace, Is.EqualTo(new[] { "update:initial" }));
        }

        [Test]
        public void SparseIntegerStateIDs_AreSupportedByDirectIndexedStorage()
        {
            var definition = new IntegerFsm(23003);
            definition.AddState(new IntegerFsmState(3, null, null, null));
            definition.AddState(new IntegerFsmState(17, null, null, null));

            Assert.That(definition.HasState(3), Is.True);
            Assert.That(definition.HasState(17), Is.True);
            Assert.That(definition.HasState(4), Is.False);
            Assert.That(definition.GetAllStates().Count, Is.EqualTo(2));
        }

        [Test]
        public void IntegerTimers_MirrorStringTimerOperations()
        {
            const int floatTimerID = 23004;
            const int intTimerID = 23005;

            IntegerFsmTimers.RemoveFloatTimer(floatTimerID);
            IntegerFsmTimers.RemoveIntTimer(intTimerID);

            try
            {
                IntegerFsmTimers.AddOrSetFloatTimer(floatTimerID, 2.5f);
                IntegerFsmTimers.AddOrSetIntTimer(intTimerID, 5);
                IntegerFsmTimers.UpdateTimers(0.5f, 2);

                Assert.That(IntegerFsmTimers.FloatTimers[floatTimerID], Is.EqualTo(2.0f).Within(0.0001f));
                Assert.That(IntegerFsmTimers.IntTimers[intTimerID], Is.EqualTo(3));

                IntegerFsmTimers.ResetFloatTimer(floatTimerID, 9f);
                IntegerFsmTimers.ResetIntTimer(intTimerID, 11);
                Assert.That(IntegerFsmTimers.FloatTimers[floatTimerID], Is.EqualTo(9f));
                Assert.That(IntegerFsmTimers.IntTimers[intTimerID], Is.EqualTo(11));
            }
            finally
            {
                IntegerFsmTimers.RemoveFloatTimer(floatTimerID);
                IntegerFsmTimers.RemoveIntTimer(intTimerID);
            }
        }
    }
}
