using NUnit.Framework;
using System;
using TheSingularityWorkshop.FSM_API.IntegerBacked;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMBuilderInt_Tests
    {
        [Test]
        public void State_Transition_AndInitialState_BuildIntoIntegerDefinition()
        {
            var builder = new FSMBuilderInt(7, processRate: 3, processingGroupID: 2)
                .State(10)
                .State(20)
                .WithInitialState(10)
                .Transition(10, 20, _ => true)
                .AnyTransition(10, _ => false);

            var definition = builder.BuildDefinition();

            Assert.That(definition.FSM_ID, Is.EqualTo(7));
            Assert.That(definition.ProcessRate, Is.EqualTo(3));
            Assert.That(definition.ProcessingGroupID, Is.EqualTo(2));
            Assert.That(definition.InitialStateID, Is.EqualTo(10));
            Assert.That(definition.HasState(10), Is.True);
            Assert.That(definition.HasState(20), Is.True);
            Assert.That(definition.HasTransition(10, 20), Is.True);
            Assert.That(definition.HasTransition(FSM.AnyStateIdentifier, 10), Is.True);
        }

        [Test]
        public void DuplicateStateID_IsRejected()
        {
            var builder = new FSMBuilderInt(1);
            builder.State(5);

            Assert.That(() => builder.State(5), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void UndefinedInitialState_IsRejectedAtBuild()
        {
            var builder = new FSMBuilderInt(1)
                .State(0)
                .WithInitialState(99);

            Assert.That(() => builder.BuildDefinition(), Throws.TypeOf<InvalidOperationException>());
        }
    }
}
