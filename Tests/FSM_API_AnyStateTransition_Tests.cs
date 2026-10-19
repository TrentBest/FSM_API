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
                .Transition(FSM.AnyStateIdentifier, "B", _ => { trace.Add("guard:any:B"); return true; })
                .Transition(FSM.AnyStateIdentifier, "C", _ => { trace.Add("guard:any:C"); return true; })
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
    }
}
