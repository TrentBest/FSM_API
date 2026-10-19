using System.Reflection;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandleInt = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public sealed class FSMRuntime_ReplacementLifecycle_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; } = "ReplacementContext";
            public int Context_ID => 0;
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void Register_ReplacementMarksRemovedHandlesUnregistered()
        {
            var runtime = new IntegerFSMRuntime();
            var definition = new IntegerFSM(7, -1);
            definition.AddState(new IntegerFSMState(0, null, null, null));
            runtime.Register(definition);
            IntegerFSMHandleInt handle = runtime.CreateInstance(7, new TestContext());

            Assert.That(GetRegistrationFlag(handle), Is.True);

            var replacement = new IntegerFSM(7, -1);
            replacement.AddState(new IntegerFSMState(0, null, null, null));
            runtime.Register(replacement);

            Assert.That(runtime.GetHandleCount(0), Is.Zero);
            Assert.That(GetRegistrationFlag(handle), Is.False,
                "Replacing a definition removes its handles, so their registration flag must be cleared too.");
        }

        private static bool GetRegistrationFlag(IntegerFSMHandleInt handle)
        {
            var property = typeof(IntegerFSMHandleInt).GetProperty(
                "IsRegistered", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, "The runtime registration flag should remain an internal implementation detail.");
            return (bool)property.GetValue(handle);
        }
    }
}
