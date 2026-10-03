using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public class FSMUtilitiesInt_Tests
    {
        [Test]
        public void IdentityValidation_MatchesIntegerRuntimeRules()
        {
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidFSMID(0), Is.True);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidFSMID(-1), Is.False);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidStateID(0), Is.True);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidStateID(-1), Is.False);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsAnyStateIdentifier(TheSingularityWorkshop.FSM_API.IntegerBacked.FSM.AnyStateIdentifier), Is.True);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsAnyStateIdentifier(0), Is.False);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidProcessingGroupID(0), Is.True);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidProcessingGroupID(-1), Is.False);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidProcessRate(-1), Is.True);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidProcessRate(0), Is.True);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidProcessRate(10), Is.True);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.IsValidProcessRate(-2), Is.False);
        }

        [Test]
        public void TryGetHelpers_AreNonThrowingForUnknownValues()
        {
            var definition = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSM(4);
            definition.AddState(new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState(0, null, null, null));
            var runtime = new TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime();
            runtime.Register(definition);

            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.TryGetState(definition, 0, out var state), Is.True);
            Assert.That(state.StateID, Is.EqualTo(0));
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.TryGetState(definition, 99, out _), Is.False);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.TryGetDefinition(runtime, 4, out var registered), Is.True);
            Assert.That(registered, Is.SameAs(definition));
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.TryGetDefinition(runtime, 99, out _), Is.False);
            Assert.That(TheSingularityWorkshop.FSM_API.IntegerBacked.FSMUtilitiesInt.TryGetDefinition(null, 4, out _), Is.False);
        }
    }
}
