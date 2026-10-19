using System.Collections.Generic;
using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public sealed class FSM_API_AnyStateTransition_Tests
    {
        private sealed class Context : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "AnyStateTransitionTests";
        }

        [SetUp]
        public void SetUp()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void AnyStateTransition_FirstMatchWins_AndPreemptsRegularTransitions()
        {
            var trace = new List<string>();
            const string fsmName = "AnyStatePrecedence";
            const string group = "AnyStatePrecedenceTests";

            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, group)
                .State("A", null, _ => trace.Add("update:A"), _ => trace.Add("exit:A"))
                .State("B", _ => trace.Add("enter:B"), null, null)
                .State("C", _ => trace.Add("enter:C"), null, null)
                .WithInitialState("A")
                .Transition("A", "C", _ => { trace.Add("guard:regular"); return true; })
                .AnyTransition("B", _ => { trace.Add("guard:any:B"); return true; })
                .AnyTransition("C", _ => { trace.Add("guard:any:C"); return true; })
                .BuildDefinition();

            var handle = FSM_API.Create.CreateInstance(fsmName, new Context(), group);
            trace.Clear();

            handle.Update(group);

            Assert.That(handle.CurrentState, Is.EqualTo("B"));
            Assert.That(trace, Is.EqualTo(new[]
            {
                "update:A",
                "guard:any:B",
                "exit:A"
            }));
        }

        [Test]
        public void AnyStateTransition_MatchWithoutRegularTransition_ChangesCurrentState()
        {
            var trace = new List<string>();
            const string fsmName = "AnyStateOnly";
            const string group = "AnyStateOnlyTests";

            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, group)
                .State("A", null, _ => trace.Add("update:A"), _ => trace.Add("exit:A"))
                .State("B", null, null, null)
                .WithInitialState("A")
                .AnyTransition("B", _ => { trace.Add("guard:any:B"); return true; })
                .BuildDefinition();

            var handle = FSM_API.Create.CreateInstance(fsmName, new Context(), group);
            trace.Clear();

            handle.Update(group);

            Assert.That(handle.CurrentState, Is.EqualTo("B"));
            Assert.That(trace, Is.EqualTo(new[]
            {
                "update:A",
                "guard:any:B",
                "exit:A"
            }));
        }

        [Test]
        public void AnyStateTransition_NoMatch_AllowsRegularTransition()
        {
            var trace = new List<string>();
            const string fsmName = "AnyStateNoMatch";
            const string group = "AnyStateNoMatchTests";

            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, group)
                .State("A", null, _ => trace.Add("update:A"), _ => trace.Add("exit:A"))
                .State("B", null, null, null)
                .State("C", null, null, null)
                .WithInitialState("A")
                .AnyTransition("B", _ => { trace.Add("guard:any:B"); return false; })
                .Transition("A", "C", _ => { trace.Add("guard:regular"); return true; })
                .BuildDefinition();

            var handle = FSM_API.Create.CreateInstance(fsmName, new Context(), group);
            trace.Clear();

            handle.Update(group);

            Assert.That(handle.CurrentState, Is.EqualTo("C"));
            Assert.That(trace, Is.EqualTo(new[]
            {
                "update:A",
                "guard:any:B",
                "guard:regular",
                "exit:A"
            }));
        }
        [Test]
        public void AnyStateTransition_InvalidTargetIsSkippedBeforeGuardEvaluation()
        {
            var trace = new List<string>();
            const string fsmName = "AnyStateInvalidTarget";
            const string group = "AnyStateInvalidTargetTests";

            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, group)
                .State("A", null, _ => trace.Add("update:A"), _ => trace.Add("exit:A"))
                .State("B", null, null, null)
                .WithInitialState("A")
                .AnyTransition("Missing", _ => { trace.Add("guard:missing"); return true; })
                .AnyTransition("B", _ => { trace.Add("guard:any:B"); return true; })
                .BuildDefinition();

            var handle = FSM_API.Create.CreateInstance(fsmName, new Context(), group);
            trace.Clear();

            handle.Update(group);

            Assert.That(handle.CurrentState, Is.EqualTo("B"));
            Assert.That(trace, Is.EqualTo(new[]
            {
                "update:A",
                "guard:any:B",
                "exit:A"
            }));
        }

        [Test]
        public void AnyStateTransition_IsEvaluatedByScheduledUpdate()
        {
            var trace = new List<string>();
            const string fsmName = "AnyStateScheduled";
            const string group = "AnyStateScheduledTests";

            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, group)
                .State("A", _ => trace.Add("enter:A"), _ => trace.Add("update:A"), _ => trace.Add("exit:A"))
                .State("B", _ => trace.Add("enter:B"), null, null)
                .WithInitialState("A")
                .AnyTransition("B", _ => { trace.Add("guard:any:B"); return true; })
                .BuildDefinition();

            var handle = FSM_API.Create.CreateInstance(fsmName, new Context(), group);

            FSM_API.Interaction.Update(group);

            Assert.That(handle.CurrentState, Is.EqualTo("B"));
            Assert.That(trace, Does.Contain("guard:any:B"));
            Assert.That(trace, Does.Contain("exit:A"));
        }
    }
}
