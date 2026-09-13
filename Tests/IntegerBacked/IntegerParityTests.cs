using NUnit.Framework;
using TheSingularityWorkshop.FSM_API.IntegerBacked;

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
            var definition = new FSM(23001);
            definition.AddState(new FSMState(0, null, null, null));
            definition.AddState(new FSMState(1, null, null, null));
            definition.AddState(new FSMState(2, null, null, null));
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
            var definition = new FSM(23002);
            definition.AddState(new FSMState(0, _ => entered++, null, _ => exited++));
            definition.AddState(new FSMState(1, _ => entered++, null, _ => exited++));

            var handle = new FSMHandleInt(definition, new Context());
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
        public void SparseIntegerStateIDs_AreSupportedByDirectIndexedStorage()
        {
            var definition = new FSM(23003);
            definition.AddState(new FSMState(3, null, null, null));
            definition.AddState(new FSMState(17, null, null, null));

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

            FSMTimersInt.RemoveFloatTimer(floatTimerID);
            FSMTimersInt.RemoveIntTimer(intTimerID);

            try
            {
                FSMTimersInt.AddOrSetFloatTimer(floatTimerID, 2.5f);
                FSMTimersInt.AddOrSetIntTimer(intTimerID, 5);
                FSMTimersInt.UpdateTimers(0.5f, 2);

                Assert.That(FSMTimersInt.FloatTimers[floatTimerID], Is.EqualTo(2.0f).Within(0.0001f));
                Assert.That(FSMTimersInt.IntTimers[intTimerID], Is.EqualTo(3));

                FSMTimersInt.ResetFloatTimer(floatTimerID, 9f);
                FSMTimersInt.ResetIntTimer(intTimerID, 11);
                Assert.That(FSMTimersInt.FloatTimers[floatTimerID], Is.EqualTo(9f));
                Assert.That(FSMTimersInt.IntTimers[intTimerID], Is.EqualTo(11));
            }
            finally
            {
                FSMTimersInt.RemoveFloatTimer(floatTimerID);
                FSMTimersInt.RemoveIntTimer(intTimerID);
            }
        }
    }
}
