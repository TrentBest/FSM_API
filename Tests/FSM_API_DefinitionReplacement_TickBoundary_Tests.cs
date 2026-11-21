using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public sealed class FSM_API_DefinitionReplacement_TickBoundary_Tests
    {
        private sealed class TestContext : IStateContext
        {
            public string Name { get; set; } = "TickBoundary";
            public int Context_ID => 0;
            public bool IsValid { get; set; } = true;
        }

        [SetUp]
        public void SetUp()
        {
            FsmApi.Internal.ResetAPI(true);
        }

        [TearDown]
        public void TearDown()
        {
            FsmApi.Internal.ResetAPI(true);
        }

        [Test]
        public void RegisterFromEnter_IsDeferredUntilEveryStringHandleCompletesTheTick()
        {
            const string name = "StringReplacementTickBoundary";
            const string group = "StringReplacementGroup";
            var trace = new List<string>();
            var replacementRequested = false;
            FSM replacement = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", _ =>
                {
                    trace.Add("old.enter");
                    if (replacementRequested)
                    {
                        return;
                    }

                    replacementRequested = true;
                    replacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                        .State("A", _ => trace.Add("new.enter"), _ => trace.Add("new.update"), null)
                        .BuildDefinition();
                }, _ => trace.Add("old.update"), null)
                .BuildDefinition();

            var first = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            var second = FsmApi.Create.CreateInstance(name, new TestContext(), group);

            FsmApi.Interaction.Update(group);

            Assert.That(first.Definition, Is.SameAs(replacement));
            Assert.That(second.Definition, Is.SameAs(replacement));
            Assert.That(replacement.ProcessingGroup, Is.EqualTo(group));
            Assert.That(replacement.ProcessRate, Is.EqualTo(-1));
            Assert.That(trace, Is.EqualTo(new[]
            {
                "old.enter", "old.update",
                "old.enter", "old.update"
            }), "Every string handle selected for this tick must finish against the old definition.");

            FsmApi.Interaction.Update(group);

            Assert.That(trace, Is.EqualTo(new[]
            {
                "old.enter", "old.update",
                "old.enter", "old.update",
                "new.update", "new.update"
            }), "The replacement must become visible only after the tick boundary.");
        }

        [Test]
        public void DirectHandleUpdate_UsesTheStringRuntimeMutationBoundary()
        {
            const string name = "StringManualReplacementBoundary";
            const string group = "StringManualGroup";
            var replacementRequested = false;
            FSM replacement = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ =>
                {
                    if (replacementRequested)
                    {
                        return;
                    }

                    replacementRequested = true;
                    replacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                        .State("A", null, null, null)
                        .BuildDefinition();
                }, null)
                .BuildDefinition();

            var handle = FsmApi.Create.CreateInstance(name, new TestContext(), group);

            handle.Update();

            Assert.That(handle.Definition, Is.SameAs(replacement));
            Assert.That(replacement.ProcessingGroup, Is.EqualTo(group));
            Assert.That(replacement.ProcessRate, Is.EqualTo(-1));
            Assert.That(handle.IsDestroyed, Is.False);
        }

        [Test]
        public void NestedStringSchedulerUpdate_IsRejectedWithoutStartingAnotherTick()
        {
            const string name = "StringNestedUpdateBoundary";
            const string group = "StringNestedGroup";
            var nestedUpdateRejected = false;
            var updateCount = 0;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ =>
                {
                    updateCount++;
                    try
                    {
                        FsmApi.Interaction.Update(group);
                    }
                    catch (InvalidOperationException)
                    {
                        nestedUpdateRejected = true;
                    }
                }, null)
                .BuildDefinition();

            FsmApi.Create.CreateInstance(name, new TestContext(), group);

            FsmApi.Interaction.Update(group);

            Assert.That(nestedUpdateRejected, Is.True);
            Assert.That(updateCount, Is.EqualTo(1));
        }

        [Test]
        public void ExplicitDestroyAfterQueuedReplacement_CancelsThePendingStringRegistration()
        {
            const string name = "StringUnregisterOrdering";
            const string group = "StringUnregisterGroup";
            var replacementRequested = false;
            var handle = (FSMHandle)null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ =>
                {
                    if (replacementRequested)
                    {
                        return;
                    }

                    replacementRequested = true;
                    FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                        .State("A", null, null, null)
                        .BuildDefinition();
                    FsmApi.Interaction.DestroyFiniteStateMachine(name, group);
                }, null)
                .BuildDefinition();

            handle = FsmApi.Create.CreateInstance(name, new TestContext(), group);

            FsmApi.Interaction.Update(group);

            Assert.That(handle.IsDestroyed, Is.True);
            Assert.That(FsmApi.Internal.GetFsmDefinition(name, group), Is.Null);
        }

        [Test]
        public void ReplacementWithoutCurrentState_RecoversThroughTheNewInitialState()
        {
            const string name = "StringReplacementStateRecovery";
            const string group = "StringReplacementRecoveryGroup";
            var trace = new List<string>();

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ => trace.Add("old.update"), null)
                .BuildDefinition();

            var handle = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            FsmApi.Interaction.Update(group);

            var replacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("B", _ => trace.Add("new.enter"), _ => trace.Add("new.update"), null)
                .BuildDefinition();

            FsmApi.Interaction.Update(group);
            Assert.That(handle.CurrentState, Is.EqualTo("B"));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(trace, Is.EqualTo(new[] { "old.update" }));

            FsmApi.Interaction.Update(group);
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "new.enter", "new.update" }));
        }
        [Test]
        public void RegisterFromScheduledUpdate_IsDeferredUntilEveryStringHandleCompletesTheTick()
        {
            const string name = "StringUpdateReplacementBoundary";
            const string group = "StringUpdateReplacementGroup";
            var trace = new List<string>();
            var requested = false;
            FSM replacement = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ =>
                {
                    trace.Add("old.update");
                    if (!requested)
                    {
                        requested = true;
                        replacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                            .State("A", null, _ => trace.Add("new.update"), null)
                            .BuildDefinition();
                    }
                }, null)
                .BuildDefinition();

            var first = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            var second = FsmApi.Create.CreateInstance(name, new TestContext(), group);

            FsmApi.Interaction.Update(group);

            Assert.That(first.Definition, Is.SameAs(replacement));
            Assert.That(second.Definition, Is.SameAs(replacement));
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update" }));

            FsmApi.Interaction.Update(group);
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update", "new.update", "new.update" }));
        }

        [Test]
        public void RegisterFromTransitionCondition_IsDeferredUntilStringTickBoundary()
        {
            const string name = "StringTransitionReplacementBoundary";
            const string group = "StringTransitionReplacementGroup";
            var trace = new List<string>();
            var requested = false;
            FSM replacement = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ => trace.Add("old.update"), null)
                .Transition("A", "A", _ =>
                {
                    if (!requested)
                    {
                        requested = true;
                        replacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                            .State("A", null, _ => trace.Add("new.update"), null)
                            .BuildDefinition();
                    }
                    return false;
                })
                .BuildDefinition();

            var first = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            var second = FsmApi.Create.CreateInstance(name, new TestContext(), group);

            FsmApi.Interaction.Update(group);

            Assert.That(first.Definition, Is.SameAs(replacement));
            Assert.That(second.Definition, Is.SameAs(replacement));
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update" }));

            FsmApi.Interaction.Update(group);
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "old.update", "new.update", "new.update" }));
        }

        [Test]
        public void MultipleStringRegistrationsFromOneCallback_LastRequestWins()
        {
            const string name = "StringMultipleReplacementBoundary";
            const string group = "StringMultipleReplacementGroup";
            var requested = false;
            FSM lastReplacement = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ =>
                {
                    if (requested)
                    {
                        return;
                    }

                    requested = true;
                    FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                        .State("A", null, null, null)
                        .BuildDefinition();
                    lastReplacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                        .State("A", null, null, null)
                        .BuildDefinition();
                }, null)
                .BuildDefinition();

            var handle = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            FsmApi.Interaction.Update(group);

            Assert.That(handle.Definition, Is.SameAs(lastReplacement));
        }

        [Test]
        public void RegistrationRequestedBeforeStringCallbackFailure_IsCommittedAtTickBoundary()
        {
            const string name = "StringCallbackFailureReplacement";
            const string group = "StringCallbackFailureGroup";
            var requested = false;
            FSM replacement = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ =>
                {
                    if (requested)
                    {
                        return;
                    }

                    requested = true;
                    replacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                        .State("A", null, null, null)
                        .BuildDefinition();
                    throw new InvalidOperationException("Expected callback failure.");
                }, null)
                .BuildDefinition();

            var handle = FsmApi.Create.CreateInstance(name, new TestContext(), group);

            FsmApi.Interaction.Update(group);

            Assert.That(handle.Definition, Is.SameAs(replacement));
            Assert.That(handle.IsDestroyed, Is.False);
        }

        [Test]
        public void NestedStringSiblingHandleUpdate_IsRejectedAndSiblingRunsOnce()
        {
            const string name = "StringNestedSiblingBoundary";
            const string group = "StringNestedSiblingGroup";
            var nestedUpdateRejected = false;
            var siblingUpdateCount = 0;
            FSMHandle siblingHandle = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, context =>
                {
                    if (context.Name == "first")
                    {
                        try
                        {
                            siblingHandle.Update();
                        }
                        catch (InvalidOperationException)
                        {
                            nestedUpdateRejected = true;
                        }
                    }
                    else if (context.Name == "second")
                    {
                        siblingUpdateCount++;
                    }
                }, null)
                .BuildDefinition();

            FsmApi.Create.CreateInstance(name, new TestContext { Name = "first" }, group);
            siblingHandle = FsmApi.Create.CreateInstance(name, new TestContext { Name = "second" }, group);

            FsmApi.Interaction.Update(group);

            Assert.That(nestedUpdateRejected, Is.True);
            Assert.That(siblingUpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void InvalidStringReplacement_SuspendsUntilAValidDefinitionArrives()
        {
            const string name = "StringInvalidReplacementBoundary";
            const string group = "StringInvalidReplacementGroup";
            var trace = new List<string>();

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, _ => trace.Add("old.update"), null)
                .BuildDefinition();

            var handle = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            FsmApi.Interaction.Update(group);

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group).BuildDefinition();
            FsmApi.Interaction.Update(group);
            FsmApi.Interaction.Update(group);

            Assert.That(handle.IsDestroyed, Is.False);
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(trace, Is.EqualTo(new[] { "old.update" }));

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("B", _ => trace.Add("new.enter"), _ => trace.Add("new.update"), null)
                .BuildDefinition();

            FsmApi.Interaction.Update(group);
            Assert.That(handle.CurrentState, Is.EqualTo("B"));
            Assert.That(handle.HasEnteredCurrentState, Is.False);

            FsmApi.Interaction.Update(group);
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "old.update", "new.enter", "new.update" }));
        }

        [Test]
        public void RegistrationFromExternalDestroyExit_IsDeferredUntilTeardownCompletes()
        {
            const string name = "StringExternalDestroyExitBoundary";
            const string group = "StringExternalDestroyGroup";
            var replacementRequested = false;
            var nestedUpdateRejected = false;
            FSM replacement = null;
            FSMHandle secondHandle = null;

            FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                .State("A", null, null, _ =>
                {
                    if (secondHandle != null)
                    {
                        try
                        {
                            secondHandle.Update();
                        }
                        catch (InvalidOperationException)
                        {
                            nestedUpdateRejected = true;
                        }
                    }

                    if (!replacementRequested)
                    {
                        replacementRequested = true;
                        replacement = FsmApi.Create.CreateFiniteStateMachine(name, -1, group)
                            .State("B", null, null, null)
                            .BuildDefinition();
                    }
                })
                .BuildDefinition();

            var firstHandle = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            secondHandle = FsmApi.Create.CreateInstance(name, new TestContext(), group);
            FsmApi.Interaction.Update(group);

            FsmApi.Interaction.DestroyFiniteStateMachine(name, group);

            Assert.That(firstHandle.IsDestroyed, Is.True);
            Assert.That(secondHandle.IsDestroyed, Is.True);
            Assert.That(nestedUpdateRejected, Is.True,
                "An Exit callback during external teardown must not start another handle update.");
            Assert.That(FsmApi.Internal.GetFsmDefinition(name, group), Is.SameAs(replacement),
                "A replacement requested by Exit must be registered only after the old definition and its handles are torn down.");
        }

    }
}
