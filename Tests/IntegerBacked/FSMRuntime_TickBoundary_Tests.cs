using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMRuntime = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMRuntime;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandle;

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

        [Test]
        public void DirectHandleUpdate_UsesTheRuntimeBoundaryForCallbackRegistration()
        {
            var runtime = new IntegerFSMRuntime();
            var replacement = new IntegerFSM(49, -1);
            replacement.AddState(new IntegerFSMState(0, null, null, null));
            var requested = false;
            var original = new IntegerFSM(49, -1);
            original.AddState(new IntegerFSMState(0, null, _ =>
            {
                if (!requested)
                {
                    requested = true;
                    runtime.Register(replacement, 18);
                }
            }, null));

            runtime.Register(original, 18);
            var handle = runtime.CreateInstance(49, new TestContext());

            handle.Update();

            Assert.That(handle.Definition, Is.SameAs(replacement),
                "A public single-handle update must also defer callback registration until its operation completes.");
        }

        [Test]
        public void NestedSiblingHandleUpdate_IsRejectedAndSiblingRunsOnceAtItsScheduledPosition()
        {
            var runtime = new IntegerFSMRuntime();
            var nestedUpdateRejected = false;
            var siblingUpdateCount = 0;
            var sibling = new IntegerFSM(51, -1);
            sibling.AddState(new IntegerFSMState(0, null, _ => siblingUpdateCount++, null));

            var first = new IntegerFSM(50, -1);
            first.AddState(new IntegerFSMState(0, null, _ =>
            {
                try
                {
                    foreach (var siblingHandle in runtime.GetInstances(51))
                    {
                        siblingHandle.Update();
                    }
                }
                catch (InvalidOperationException)
                {
                    nestedUpdateRejected = true;
                }
            }, null));

            runtime.Register(first, 19);
            runtime.Register(sibling, 19);
            runtime.CreateInstance(50, new TestContext());
            runtime.CreateInstance(51, new TestContext());

            runtime.Update(19);

            Assert.That(nestedUpdateRejected, Is.True);
            Assert.That(siblingUpdateCount, Is.EqualTo(1),
                "The sibling must execute once from the tick-start snapshot, not recursively and again later.");
        }

        [Test]
        public void UnregisterAfterQueuedReplacement_CancelsReplacementAndDestroysExistingInstances()
        {
            var runtime = new IntegerFSMRuntime();
            var replacement = new IntegerFSM(52, -1);
            replacement.AddState(new IntegerFSMState(0, null, null, null));
            var requested = false;
            var original = new IntegerFSM(52, -1);
            original.AddState(new IntegerFSMState(0, null, _ =>
            {
                if (requested)
                {
                    return;
                }

                requested = true;
                runtime.Register(replacement, 20);
                Assert.That(runtime.Unregister(52), Is.True);
            }, null));

            runtime.Register(original, 20);
            var handle = runtime.CreateInstance(52, new TestContext());

            runtime.Update(20);

            Assert.That(handle.IsDestroyed, Is.True);
            Assert.That(runtime.Contains(52), Is.False,
                "An unregister later in the same callback sequence must cancel an earlier queued replacement.");
        }

        [Test]
        public void HandleUpdatesFromAutomaticTeardownCallbacks_AreRejectedDuringFinalization()
        {
            var previousInstanceThreshold = FsmApi.Error.InstanceErrorThreshold;
            var previousDefinitionThreshold = FsmApi.Error.DefinitionErrorThreshold;
            FsmApi.Error.InstanceErrorThreshold = 1;
            FsmApi.Error.DefinitionErrorThreshold = 1;

            try
            {
                var runtime = new IntegerFSMRuntime();
                var nestedUpdateRejected = false;
                TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt failingHandle = null;
                var definition = new IntegerFSM(55, -1);
                definition.AddState(new IntegerFSMState(
                    0,
                    null,
                    context =>
                    {
                        if (context.Name == "failing")
                        {
                            throw new InvalidOperationException("Expected threshold-triggering failure.");
                        }
                    },
                    _ =>
                    {
                        if (failingHandle == null)
                        {
                            return;
                        }

                        try
                        {
                            failingHandle.Update();
                        }
                        catch (InvalidOperationException)
                        {
                            nestedUpdateRejected = true;
                        }
                    }));

                runtime.Register(definition, 22);
                failingHandle = runtime.CreateInstance(55, new TestContext { Name = "failing" });
                var siblingHandle = runtime.CreateInstance(55, new TestContext { Name = "sibling" });

                runtime.Update(22);

                Assert.That(nestedUpdateRejected, Is.True,
                    "Teardown callbacks during tick finalization must not start another handle update.");
                Assert.That(failingHandle.IsDestroyed, Is.True);
                Assert.That(siblingHandle.IsDestroyed, Is.True);
                Assert.That(runtime.Contains(55), Is.False);
            }
            finally
            {
                FsmApi.Error.InstanceErrorThreshold = previousInstanceThreshold;
                FsmApi.Error.DefinitionErrorThreshold = previousDefinitionThreshold;
            }
        }

        [Test]
        public void HandleCreatedDuringCallback_IsExcludedFromCurrentTickAndReboundAtBoundary()
        {
            var trace = new List<string>();
            var runtime = new IntegerFSMRuntime();
            var created = false;
            IntegerFSMHandle createdHandle = null;
            var replacement = new IntegerFSM(56, -1);
            replacement.AddState(new IntegerFSMState(0, null, _ => trace.Add("new.update"), null));

            var original = new IntegerFSM(56, -1);
            original.AddState(new IntegerFSMState(0, null, _ =>
            {
                trace.Add("old.update");
                if (created)
                {
                    return;
                }

                created = true;
                createdHandle = runtime.CreateInstance(56, new TestContext());
                runtime.Register(replacement, 23);
            }, null));

            runtime.Register(original, 23);
            var firstHandle = runtime.CreateInstance(56, new TestContext());

            runtime.Update(23);

            Assert.That(createdHandle, Is.Not.Null);
            Assert.That(createdHandle.Definition, Is.SameAs(replacement));
            Assert.That(trace, Is.EqualTo(new[] { "old.update" }),
                "The callback-created handle must not be processed from the tick-start snapshot.");

            runtime.Update(23);

            Assert.That(trace, Is.EqualTo(new[] { "old.update", "new.update", "new.update" }));
            Assert.That(firstHandle.Definition, Is.SameAs(replacement));
        }

        [Test]
        public void AutomaticThresholdTeardown_RemovesOldHandlesThenAppliesQueuedReplacement()
        {
            var previousInstanceThreshold = FsmApi.Error.InstanceErrorThreshold;
            var previousDefinitionThreshold = FsmApi.Error.DefinitionErrorThreshold;
            FsmApi.Error.InstanceErrorThreshold = 1;
            FsmApi.Error.DefinitionErrorThreshold = 1;

            try
            {
                var runtime = new IntegerFSMRuntime();
                var replacement = new IntegerFSM(57, -1);
                replacement.AddState(new IntegerFSMState(0, null, null, null));
                var replacementRequested = false;
                var original = new IntegerFSM(57, -1);
                original.AddState(new IntegerFSMState(0, null, context =>
                {
                    if (context.Name == "failing")
                    {
                        throw new InvalidOperationException("Expected threshold-triggering failure.");
                    }

                    if (!replacementRequested)
                    {
                        replacementRequested = true;
                        runtime.Register(replacement, 24);
                    }
                }, null));

                runtime.Register(original, 24);
                var failingHandle = runtime.CreateInstance(57, new TestContext { Name = "failing" });
                var replacingHandle = runtime.CreateInstance(57, new TestContext { Name = "replacing" });

                runtime.Update(24);

                Assert.That(failingHandle.IsDestroyed, Is.True);
                Assert.That(replacingHandle.IsDestroyed, Is.True);
                Assert.That(runtime.Contains(57), Is.True);
                Assert.That(runtime.GetHandleCount(24), Is.Zero,
                    "Automatic teardown must remove old handles before the queued replacement is published.");
            }
            finally
            {
                FsmApi.Error.InstanceErrorThreshold = previousInstanceThreshold;
                FsmApi.Error.DefinitionErrorThreshold = previousDefinitionThreshold;
            }
        }

        [Test]
        public void InitializeCalledFromAnotherHandleCallback_IsRejectedUntilItsScheduledTurn()
        {
            var trace = new List<string>();
            var runtime = new IntegerFSMRuntime();
            IntegerFSMHandle sibling = null;
            var nestedInitializeRejected = false;

            var firstDefinition = new IntegerFSM(58, -1);
            firstDefinition.AddState(new IntegerFSMState(
                0,
                _ => trace.Add("first.enter"),
                _ =>
                {
                    trace.Add("first.update");
                    try
                    {
                        sibling.Initialize();
                    }
                    catch (InvalidOperationException)
                    {
                        nestedInitializeRejected = true;
                    }
                },
                null));

            var siblingDefinition = new IntegerFSM(59, -1);
            siblingDefinition.AddState(new IntegerFSMState(
                0,
                _ => trace.Add("sibling.enter"),
                _ => trace.Add("sibling.update"),
                null));

            runtime.Register(firstDefinition, 25);
            runtime.Register(siblingDefinition, 25);
            runtime.CreateInstance(58, new TestContext());
            sibling = runtime.CreateInstance(59, new TestContext());

            runtime.Update(25);

            Assert.That(nestedInitializeRejected, Is.True,
                "Initialize must use the same runtime operation scope as other public handle operations.");
            Assert.That(trace, Is.EqualTo(new[]
            {
                "first.enter",
                "first.update",
                "sibling.enter",
                "sibling.update"
            }), "The sibling's Enter callback must run only when the scheduler reaches that handle.");
        }

    }
}
