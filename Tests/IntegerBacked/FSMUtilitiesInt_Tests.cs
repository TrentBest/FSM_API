using NUnit.Framework;
using TheSingularityWorkshop.FSM_API.IntegerBacked;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMUtilitiesInt_Tests
    {
        [Test]
        public void IdentityValidation_MatchesIntegerRuntimeRules()
        {
            Assert.That(FSMUtilitiesInt.IsValidFSMID(0), Is.True);
            Assert.That(FSMUtilitiesInt.IsValidFSMID(-1), Is.False);
            Assert.That(FSMUtilitiesInt.IsValidStateID(0), Is.True);
            Assert.That(FSMUtilitiesInt.IsValidStateID(-1), Is.False);
            Assert.That(FSMUtilitiesInt.IsAnyStateIdentifier(FSM.AnyStateIdentifier), Is.True);
            Assert.That(FSMUtilitiesInt.IsAnyStateIdentifier(0), Is.False);
            Assert.That(FSMUtilitiesInt.IsValidProcessingGroupID(0), Is.True);
            Assert.That(FSMUtilitiesInt.IsValidProcessingGroupID(-1), Is.False);
            Assert.That(FSMUtilitiesInt.IsValidProcessRate(-1), Is.True);
            Assert.That(FSMUtilitiesInt.IsValidProcessRate(0), Is.True);
            Assert.That(FSMUtilitiesInt.IsValidProcessRate(10), Is.True);
            Assert.That(FSMUtilitiesInt.IsValidProcessRate(-2), Is.False);
        }

        [Test]
        public void TryGetHelpers_AreNonThrowingForUnknownValues()
        {
            var definition = new FSM(4);
            definition.AddState(new FSMState(0, null, null, null));
            var runtime = new FSMRuntime();
            runtime.Register(definition);

            Assert.That(FSMUtilitiesInt.TryGetState(definition, 0, out var state), Is.True);
            Assert.That(state.StateID, Is.EqualTo(0));
            Assert.That(FSMUtilitiesInt.TryGetState(definition, 99, out _), Is.False);
            Assert.That(FSMUtilitiesInt.TryGetDefinition(runtime, 4, out var registered), Is.True);
            Assert.That(registered, Is.SameAs(definition));
            Assert.That(FSMUtilitiesInt.TryGetDefinition(runtime, 99, out _), Is.False);
            Assert.That(FSMUtilitiesInt.TryGetDefinition(null, 4, out _), Is.False);
        }
    }
}
