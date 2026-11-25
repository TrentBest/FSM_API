using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using UnifiedApi = FSM_API.FsmApi;
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

            UnifiedApi.Interact.UpdateInstance(handle);
            Assert.That(handle.CurrentState, Is.EqualTo("A"));
            Assert.That(trace, Is.EqualTo(new[] { "A.Enter", "A.Update" }));

            context.ShouldTransition = true;
            UnifiedApi.Interact.EvaluateConditions(handle);
            Assert.That(handle.CurrentState, Is.EqualTo("B"));

            UnifiedApi.Interact.TransitionTo(handle, "A");
            Assert.That(handle.CurrentState, Is.EqualTo("A"));

            UnifiedApi.Interact.TransitionTo(handle, "B");
            UnifiedApi.Interact.ResetInstance(handle);
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

            UnifiedApi.Interact.UpdateInstance(handle);
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(trace, Is.EqualTo(new[] { "A.Enter", "A.Update" }));

            context.ShouldTransition = true;
            UnifiedApi.Interact.EvaluateConditions(handle);
            Assert.That(handle.CurrentStateID, Is.EqualTo(1));

            UnifiedApi.Interact.TransitionTo(handle, 0);
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));

            UnifiedApi.Interact.TransitionTo(handle, 1);
            UnifiedApi.Interact.ResetInstance(handle);
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
        }

        [Test]
        public void UnifiedInteractHandleOperations_RejectNullHandlesConsistently()
        {
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.UpdateInstance((StringFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.UpdateInstance((IntegerFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.EvaluateConditions((StringFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.EvaluateConditions((IntegerFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.TransitionTo((StringFSMHandle)null, "A"));
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.TransitionTo((IntegerFSMHandle)null, 0));
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.ResetInstance((StringFSMHandle)null));
            Assert.Throws<System.ArgumentNullException>(() => UnifiedApi.Interact.ResetInstance((IntegerFSMHandle)null));
        }
    }
}
