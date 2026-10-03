using NUnit.Framework;
using TheSingularityWorkshop.FSM_API.IntegerBacked;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMTimersInt_Tests
    {
        [SetUp]
        public void SetUp()
        {
            FSMTimersInt.FloatTimers.Clear();
            FSMTimersInt.IntTimers.Clear();
        }

        [Test]
        public void IntegerTimerUtilities_AddUpdateResetAndRemove()
        {
            FSMTimersInt.AddOrSetFloatTimer(10, 5f);
            FSMTimersInt.AddOrSetIntTimer(20, 8);

            FSMTimersInt.UpdateTimers(1.5f, 3);

            Assert.That(FSMTimersInt.FloatTimers[10], Is.EqualTo(3.5f).Within(0.0001f));
            Assert.That(FSMTimersInt.IntTimers[20], Is.EqualTo(5));

            FSMTimersInt.ResetFloatTimer(10, 9f);
            FSMTimersInt.ResetIntTimer(20, 11);

            Assert.That(FSMTimersInt.FloatTimers[10], Is.EqualTo(9f));
            Assert.That(FSMTimersInt.IntTimers[20], Is.EqualTo(11));
            Assert.That(FSMTimersInt.RemoveFloatTimer(10), Is.True);
            Assert.That(FSMTimersInt.RemoveIntTimer(20), Is.True);
            Assert.That(FSMTimersInt.RemoveFloatTimer(10), Is.False);
        }
    }
}
