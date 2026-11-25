using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using IntegerFSM = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFSMHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt;
using IntegerFSMState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using StringFSM = TheSingularityWorkshop.FSM_API.FSM;
using StringFSMHandle = TheSingularityWorkshop.FSM_API.FSMHandle;
using StringFSMState = TheSingularityWorkshop.FSM_API.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public sealed class FsmApiHandleFacadeParityTests
    {
        private sealed class Context : IStateContext
        {
            public string Name { get; set; } = "FacadeParity";
            public bool IsValid { get; set; } = true;
            public bool ShouldTransition { get; set; }
        }

        [Test]
        public void StringBackedHandleOperations_AreReachableThroughUnifiedInteractFacade()
        {
            var trace = new List<string>();
            var context = new Context();
            var definition = new StringFSM { Name = "FacadeStringParity" };
            definition.AddState(new StringFSMState(
                "A",
                _ => trace.Add("A.Enter"),
                _ => trace.Add("A.Update"),
                _ => trace.Add("A.Exit")));
            definition.AddState(new StringFSMState(
                "B",
                _ => trace.Add("B.Enter"),
                _ => trace.Add("B.Update"),
                _ => trace.Add("B.Exit")));
            definition.AddTransition("A", "B", stateContext => ((Context)stateContext).ShouldTransition);

            var handle = new StringFSMHandle(definition, context);

            FsmApi.Interact.UpdateInstance(handle);
            Assert.That(handle.CurrentState, Is.EqualTo("A"));
            Assert.That(trace, Is.EqualTo(new[] { "A.Enter", "A.Update" }));

            context.ShouldTransition = true;
            FsmApi.Interact.EvaluateConditions(handle);
            Assert.That(handle.CurrentState, Is.EqualTo("B"));

            FsmApi.Interact.TransitionTo(handle, "A");
            Assert.That(handle.CurrentState, Is.EqualTo("A"));

            FsmApi.Interact.TransitionTo(handle, "B");
            FsmApi.Interact.ResetInstance(handle);
            Assert.That(handle.CurrentState, Is.EqualTo("A"));
        }

        [Test]
        public void IntegerBackedHandleOperations_AreReachableThroughUnifiedInteractFacade()
        {
            var trace = new List<string>();
            var context = new Context();
            var definition = new IntegerFSM(31_001, -1);
            definition.AddState(new IntegerFSMState(
                0,
                _ => trace.Add("A.Enter"),
                _ => trace.Add("A.Update"),
                _ => trace.Add("A.Exit")));
            definition.AddState(new IntegerFSMState(
                1,
                _ => trace.Add("B.Enter"),
                _ => trace.Add("B.Update"),
                _ => trace.Add("B.Exit")));
            definition.AddTransition(0, 1, stateContext => ((Context)stateContext).ShouldTransition);

            var handle = new IntegerFSMHandle(definition, context);

            FsmApi.Interact.UpdateInstance(handle);
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(trace, Is.EqualTo(new[] { "A.Enter", "A.Update" }));

            context.ShouldTransition = true;
            FsmApi.Interact.EvaluateConditions(handle);
            Assert.That(handle.CurrentStateID, Is.EqualTo(1));

            FsmApi.Interact.TransitionTo(handle, 0);
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));

            FsmApi.Interact.TransitionTo(handle, 1);
            FsmApi.Interact.ResetInstance(handle);
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
        }

        [Test]
        public void UnifiedInteractHandleOperations_RejectNullHandlesConsistently()
        {
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.UpdateInstance((StringFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.UpdateInstance((IntegerFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.EvaluateConditions((StringFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.EvaluateConditions((IntegerFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.TransitionTo((StringFSMHandle)null, "A"));
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.TransitionTo((IntegerFSMHandle)null, 0));
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.ResetInstance((StringFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => FsmApi.Interact.ResetInstance((IntegerFSMHandle)null));
        }
    }
}
