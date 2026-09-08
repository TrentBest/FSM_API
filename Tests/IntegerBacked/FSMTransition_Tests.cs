using System;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSMTransition = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMTransition;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMTransition_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; }
            public int Context_ID => Name == null ? 0 : Name.GetHashCode();
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void Constructor_PreservesIntegerEndpoints()
        {
            var transition = new IntegerFSMTransition(10, 20, _ => true);

            Assert.That(transition.FromID, Is.EqualTo(10));
            Assert.That(transition.ToID, Is.EqualTo(20));
        }

        [Test]
        public void Constructor_AllowsZeroEndpoints()
        {
            var transition = new IntegerFSMTransition(0, 0, _ => true);

            Assert.That(transition.FromID, Is.Zero);
            Assert.That(transition.ToID, Is.Zero);
        }

        [Test]
        public void Constructor_RejectsNullCondition()
        {
            Assert.Throws<ArgumentNullException>(() => new IntegerFSMTransition(1, 2, null));
        }

        [Test]
        public void Evaluate_ReturnsConditionResult()
        {
            var transition = new IntegerFSMTransition(1, 2, context => context.IsValid);
            var context = new TestContext { Name = "valid" };

            Assert.That(transition.Evaluate(context), Is.True);

            context.IsValid = false;
            Assert.That(transition.Evaluate(context), Is.False);
        }

        [Test]
        public void Condition_CanBeReplaced()
        {
            var transition = new IntegerFSMTransition(1, 2, _ => false);

            transition.Condition = _ => true;

            Assert.That(transition.Evaluate(new TestContext()), Is.True);
        }

        [Test]
        public void ToString_UsesIntegerEndpoints()
        {
            var transition = new IntegerFSMTransition(7, 42, _ => true);

            Assert.That(transition.ToString(), Is.EqualTo("7 --[Condition]--> 42"));
        }
    }
}
