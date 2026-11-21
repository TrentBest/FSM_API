using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public sealed class FSMRuntime_TickBoundary_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; } = "TickBoundary";
            public int Context_ID => 0;
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void RegisterFromEnter_IsDeferredUntilEveryHandleCompletesTheTick()
        {
            var trace = new List<string>();
            var runtime = new IntegerFSMRuntime();
            var replacementRequested = false;
            var replacement = new IntegerFSM(41, -1);
            replacement.AddState(new IntegerFSMState(
                0,
                _ => trace.Add("new.enter"),
                _ => trace.Add("new.update"),
                null));

            var original = new IntegerFSM(41, -1);
            original.AddState(new IntegerFSMState(
                0,
                _ =>
                {
                    trace.Add("old.enter");
                    if (!replacementRequested)
                    {
                        replacementRequested = true;
                        runtime.Register(replacement, 9);
                    }
                },
                _ => trace.Add("old.update"),
                null));

            runtime.Register(original, 9);
            var first = runtime.CreateInstance(41, new TestContext());
            var second = runtime.CreateInstance(41, new TestContext());

            runtime.Update(9);

            Assert.That(first.Definition, Is.SameAs(replacement));
            Assert.That(second.Definition, Is.SameAs(replacement));
            Assert.That(trace, Is.EqualTo(new[]
            {
                "old.enter", "old.update",
                "old.enter", "old.update"
            }), "Every handle in the tick must finish against the original definition.");

            runtime.Update(9);

            Assert.That(trace, Is.EqualTo(new[]
            {
                "old.enter", "old.update",
                "old.enter", "old.update",
                "new.update", "new.update"
            }), "A same-state replacement preserves entered lifecycle state and takes effect next tick.");
        }

        [Test]
        public void RegisterFromUpdate_IsDeferredForAllHandlesInTheTick()
        {
            var trace = new List<string>();
            var runtime = new IntegerFSMRuntime();
            var replacementRequested = false;
            var replacement = new IntegerFSM(42, -1);
            replacement.AddState(new IntegerFSMState(0, null, _ => trace.Add("new.update"), null));

            var original = new IntegerFSM(42, -1);
            original.AddState(new IntegerFSMState(
                0,
                null,
                _ =>
                {
                    trace.Add("old.update");
                    if (!replacementRequested)
                    {
                        replacementRequested = true;
                        runtime.Register(replacement, 10);
                    }
                },
                null));

            runtime.Register(original, 10);
            runtime.CreateInstance(42, new TestContext());
            runtime.CreateInstance(42, new TestContext());

            runtime.Update(10);

            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update" }));
            runtime.Update(10);
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update", "new.update", "new.update" }));
        }

        [Test]
        public void RegisterFromTransitionCondition_IsDeferredUntilTickBoundary()
        {
            var trace = new List<string>();
            var runtime = new IntegerFSMRuntime();
            var replacementRequested = false;
            var replacement = new IntegerFSM(43, -1);
            replacement.AddState(new IntegerFSMState(0, null, _ => trace.Add("new.update"), null));

            var original = new IntegerFSM(43, -1);
            original.AddState(new IntegerFSMState(0, null, _ => trace.Add("old.update"), null));
            original.AddTransition(0, 0, _ =>
            {
                if (!replacementRequested)
                {
                    replacementRequested = true;
                    runtime.Register(replacement, 11);
                }
                return false;
            });

            runtime.Register(original, 11);
            var first = runtime.CreateInstance(43, new TestContext());
            var second = runtime.CreateInstance(43, new TestContext());

            runtime.Update(11);

            Assert.That(first.Definition, Is.SameAs(replacement));
            Assert.That(second.Definition, Is.SameAs(replacement));
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update" }));

            runtime.Update(11);
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update", "new.update", "new.update" }));
        }

        [Test]
        public void ReentrantUpdate_IsRejectedWithoutStartingANestedTick()
        {
            var runtime = new IntegerFSMRuntime();
            var nestedUpdateRejected = false;
            var updateCount = 0;
            var definition = new IntegerFSM(44, -1);
            definition.AddState(new IntegerFSMState(0, null, _ =>
            {
                updateCount++;
                try
                {
                    runtime.Update(12);
                }
                catch (InvalidOperationException)
                {
                    nestedUpdateRejected = true;
                }
            }, null));

            runtime.Register(definition, 12);
            runtime.CreateInstance(44, new TestContext());

            runtime.Update(12);

            Assert.That(nestedUpdateRejected, Is.True);
            Assert.That(updateCount, Is.EqualTo(1), "A callback must not recursively start a second scheduler pass.");
        }

        [Test]
        public void RegistrationRequestedBeforeCallbackFailure_IsStillCommittedAtTickBoundary()
        {
            var runtime = new IntegerFSMRuntime();
            var replacement = new IntegerFSM(45, -1);
            replacement.AddState(new IntegerFSMState(0, null, null, null));
            var requested = false;
            var original = new IntegerFSM(45, -1);
            original.AddState(new IntegerFSMState(0, null, _ =>
            {
                if (requested)
                {
                    return;
                }

                requested = true;
                runtime.Register(replacement, 13);
                throw new InvalidOperationException("Expected callback failure.");
            }, null));

            runtime.Register(original, 13);
            var handle = runtime.CreateInstance(45, new TestContext());

            runtime.Update(13);

            Assert.That(handle.Definition, Is.SameAs(replacement),
                "The tick-finalization path must commit queued registrations even when a callback fails.");
            Assert.That(handle.IsDestroyed, Is.False);
        }
        [Test]
        public void MultipleRegistrationsForSameIdentity_LastRequestWins()
        {
            var runtime = new IntegerFSMRuntime();
            var firstReplacement = new IntegerFSM(46, -1);
            firstReplacement.AddState(new IntegerFSMState(0, null, null, null));
            var lastReplacement = new IntegerFSM(46, -1);
            lastReplacement.AddState(new IntegerFSMState(0, null, null, null));
            var requested = false;
            var original = new IntegerFSM(46, -1);
            original.AddState(new IntegerFSMState(0, null, _ =>
            {
                if (requested)
                {
                    return;
                }

                requested = true;
                runtime.Register(firstReplacement, 14);
                runtime.Register(lastReplacement, 15);
            }, null));

            runtime.Register(original, 13);
            var handle = runtime.CreateInstance(46, new TestContext());
            runtime.Update(13);

            Assert.That(handle.Definition, Is.SameAs(lastReplacement));
            Assert.That(runtime.GetHandleCount(14), Is.Zero);
            Assert.That(runtime.GetHandleCount(15), Is.EqualTo(1));
        }

        [Test]
        public void RegisteringSameDefinitionDuringTick_DefersProcessingGroupMutation()
        {
            var runtime = new IntegerFSMRuntime();
            var groupCountDuringCallback = -1;
            var definition = new IntegerFSM(47, -1);
            definition.AddState(new IntegerFSMState(0, null, _ =>
            {
                if (groupCountDuringCallback >= 0)
                {
                    return;
                }

                runtime.Register(definition, 99);
                groupCountDuringCallback = runtime.GetHandleCount(16);
            }, null));

            runtime.Register(definition, 16);
            runtime.CreateInstance(47, new TestContext());
            runtime.CreateInstance(47, new TestContext());

            runtime.Update(16);

            Assert.That(groupCountDuringCallback, Is.EqualTo(2),
                "The active definition's processing group must not mutate midway through its tick.");
            Assert.That(runtime.GetHandleCount(16), Is.Zero);
            Assert.That(runtime.GetHandleCount(99), Is.EqualTo(2));
        }

        [Test]
        public void ReplacementWithoutInitialState_SuspendsUntilAValidReplacementArrives()
        {
            var trace = new List<string>();
            var runtime = new IntegerFSMRuntime();
            var original = new IntegerFSM(48, -1);
            original.AddState(new IntegerFSMState(0, null, _ => trace.Add("original.update"), null));
            runtime.Register(original, 17);
            var handle = runtime.CreateInstance(48, new TestContext());
            runtime.Update(17);

            var invalidReplacement = new IntegerFSM(48, -1);
            runtime.Register(invalidReplacement, 17);
            runtime.Update(17);
            runtime.Update(17);

            Assert.That(handle.IsDestroyed, Is.False);
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(trace, Is.EqualTo(new[] { "original.update" }),
                "An unusable replacement must suspend the handle instead of repeatedly running invalid recovery.");

            var validReplacement = new IntegerFSM(48, -1);
            validReplacement.AddState(new IntegerFSMState(
                5,
                _ => trace.Add("replacement.enter"),
                _ => trace.Add("replacement.update"),
                null));
            runtime.Register(validReplacement, 17);

            runtime.Update(17);
            Assert.That(handle.CurrentStateID, Is.EqualTo(5));
            Assert.That(handle.HasEnteredCurrentState, Is.False);

            runtime.Update(17);
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[]
            {
                "original.update",
                "replacement.enter",
                "replacement.update"
            }));
        }

    }
}
