using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    /// <summary>
    /// Deliberately adversarial tests for the integer-backed runtime.
    /// These tests exercise the mutation boundary that the established string-backed
    /// implementation promises to protect with deferred mutation handling.
    /// </summary>
    [TestFixture]
    public sealed class FSMRuntime_Stress_Tests
    {
        [Test]
        public void Update_SelfRemovalMustNotSkipTheNextHandle()
        {
            var runtime = new IntegerFSMRuntime();
            var firstUpdated = false;
            var secondUpdated = false;
            IntegerFSMRuntimeTestContext firstContext = null;

            var definition = new IntegerFSM(1, -1);
            definition.AddState(new IntegerFSMState(
                10,
                null,
                context =>
                {
                    var typed = (IntegerFSMRuntimeTestContext)context;
                    if (ReferenceEquals(typed, firstContext))
                    {
                        firstUpdated = true;
                        runtime.RemoveInstance(typed.Handle);
                    }
                    else
                    {
                        secondUpdated = true;
                    }
                },
                null));

            runtime.Register(definition, 7);
            firstContext = new IntegerFSMRuntimeTestContext();
            var firstHandle = runtime.CreateInstance(1, firstContext);
            firstContext.Handle = firstHandle;
            runtime.CreateInstance(1, new IntegerFSMRuntimeTestContext());

            runtime.Update(7);

            Assert.That(firstUpdated, Is.True);
            Assert.That(secondUpdated, Is.True,
                "Removing the currently executing handle must not cause the next live handle to be skipped.");
        }

        [Test]
        public void Update_NewHandlesMustNotRunUntilTheNextTick()
        {
            var runtime = new IntegerFSMRuntime();
            var updateCount = 0;
            var created = false;

            var definition = new IntegerFSM(2, -1);
            definition.AddState(new IntegerFSMState(
                10,
                null,
                _ =>
                {
                    updateCount++;
                    if (!created)
                    {
                        created = true;
                        runtime.CreateInstance(2, new IntegerFSMRuntimeTestContext());
                    }
                },
                null));

            runtime.Register(definition, 8);
            runtime.CreateInstance(2, new IntegerFSMRuntimeTestContext());

            runtime.Update(8);

            Assert.That(updateCount, Is.EqualTo(1),
                "A handle created during an update must be deferred rather than executed in the same tick.");
            Assert.That(runtime.GetHandleCount(8), Is.EqualTo(2));

            runtime.Update(8);

            Assert.That(updateCount, Is.EqualTo(3));
        }

        private sealed class IntegerFSMRuntimeTestContext : IStateContext
        {
            public string Name { get; set; } = "StressContext";
            public int Context_ID => 0;
            public bool IsValid { get; set; } = true;
            public TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt Handle { get; set; }
        }
    }
}
