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
        public void Register_ReplacementPreservesLiveHandlesAndCallerOwnedContexts()
        {
            var exits = 0;
            var context = new TestContext();
            var runtime = new IntegerFSMRuntime();
            var definition = new IntegerFSM(7, -1);
            definition.AddState(new IntegerFSMState(0, null, null, _ => exits++));
            runtime.Register(definition);
            IntegerFSMHandleInt handle = runtime.CreateInstance(7, context);

            Assert.That(GetRegistrationFlag(handle), Is.True);

            var replacement = new IntegerFSM(7, -1);
            replacement.AddState(new IntegerFSMState(0, null, null, null));
            runtime.Register(replacement);

            Assert.That(runtime.GetHandleCount(0), Is.EqualTo(1));
            Assert.That(GetRegistrationFlag(handle), Is.True,
                "Replacing a definition must leave the existing handle registered.");
            Assert.That(handle.IsDestroyed, Is.False);
            Assert.That(handle.Definition, Is.SameAs(replacement));
            Assert.That(handle.Context, Is.SameAs(context),
                "The runtime must preserve the caller-owned context reference.");
            Assert.That(exits, Is.Zero,
                "Definition replacement must not trigger instance teardown or invoke Exit.");
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
