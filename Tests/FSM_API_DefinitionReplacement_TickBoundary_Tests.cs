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
    }
}
