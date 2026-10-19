using NUnit.Framework;
using TheSingularityWorkshop.FSM_API.IntegerBacked;
using IntegerFsm = TheSingularityWorkshop.FSM_API.IntegerBacked.FSM;
using IntegerFsmState = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMState;
using IntegerFsmHandle = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMHandleInt;
using IntegerFsmTimers = TheSingularityWorkshop.FSM_API.IntegerBacked.FSMTimersInt;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public sealed class IntegerParityTests
    {
        private sealed class Context : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "IntegerParityTests";
            public int Context_ID => 23;
            public bool HasEnteredCurrentState { get; set; }
        }

        [Test]
        public void AnyStateTransition_TakesPriorityOverStateSpecificTransition()
        {
            var definition = new IntegerFsm(23001);
            definition.AddState(new IntegerFsmState(0, null, null, null));
            definition.AddState(new IntegerFsmState(1, null, null, null));
            definition.AddState(new IntegerFsmState(2, null, null, null));
            definition.AddTransition(0, 1, _ => true);
            definition.AddAnyStateTransition(2, _ => true);

            var result = definition.Step(0, new Context());

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void Handle_ForceTransitionAndReset_PreserveLifecycleSemantics()
        {
            var entered = 0;
            var exited = 0;
            var definition = new IntegerFsm(23002);
            definition.AddState(new IntegerFsmState(0, _ => entered++, null, _ => exited++));
            definition.AddState(new IntegerFsmState(1, _ => entered++, null, _ => exited++));

            var handle = new IntegerFsmHandle(definition, new Context());
            handle.Initialize();
            Assert.That(entered, Is.EqualTo(1));

            handle.TransitionTo(1);
            Assert.That(handle.CurrentStateID, Is.EqualTo(1));
            Assert.That(entered, Is.EqualTo(2));
            Assert.That(exited, Is.EqualTo(1));

            handle.ResetFSMInstance();
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(entered, Is.EqualTo(3));
            Assert.That(exited, Is.EqualTo(2));
        }


        [Test]
        public void Handle_NormalTransitionDefersTargetEntryUntilNextUpdate()
        {
            var trace = new System.Collections.Generic.List<string>();
            var definition = new IntegerFsm(23006);
            definition.AddState(new IntegerFsmState(0,
                _ => trace.Add("enter:A"),
                _ => trace.Add("update:A"),
                _ => trace.Add("exit:A")));
            definition.AddState(new IntegerFsmState(1,
                _ => trace.Add("enter:B"),
                _ => trace.Add("update:B"),
                _ => trace.Add("exit:B")));
            definition.AddTransition(0, 1, _ => { trace.Add("guard:A:B"); return true; });

            var handle = new IntegerFsmHandle(definition, new Context());
            handle.Initialize();
            trace.Clear();

            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(1));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(trace, Is.EqualTo(new[] { "update:A", "guard:A:B", "exit:A" }));

            trace.Clear();
            handle.Update();

            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "enter:B", "update:B" }));
        }

        [Test]
        public void Handle_InvalidCurrentStateRecovery_EntersInitialStateOnlyOnce()
        {
            var trace = new System.Collections.Generic.List<string>();
            var definition = new IntegerFsm(23007);
            definition.AddState(new IntegerFsmState(0,
                _ => trace.Add("enter:removed"),
                _ => trace.Add("update:removed"),
                _ => trace.Add("exit:removed")));
            definition.AddState(new IntegerFsmState(1,
                _ => trace.Add("enter:replacement"),
                _ => trace.Add("update:replacement"),
                _ => trace.Add("exit:replacement")));

            var handle = new IntegerFsmHandle(definition, new Context());
            handle.Initialize();
            trace.Clear();

            // Mutating the blueprint removes the running handle's current state.
            // State 1 becomes the new initial state, exercising recovery through the public API.
            definition.RemoveState(0);

            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(1));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "enter:replacement" }));

            trace.Clear();
            handle.Update();
            Assert.That(trace, Is.EqualTo(new[] { "update:replacement" }));
        }

        [Test]
        public void SparseIntegerStateIDs_AreSupportedByDirectIndexedStorage()
        {
            var definition = new IntegerFsm(23003);
            definition.AddState(new IntegerFsmState(3, null, null, null));
            definition.AddState(new IntegerFsmState(17, null, null, null));

            Assert.That(definition.HasState(3), Is.True);
            Assert.That(definition.HasState(17), Is.True);
            Assert.That(definition.HasState(4), Is.False);
            Assert.That(definition.GetAllStates().Count, Is.EqualTo(2));
        }

        [Test]
        public void IntegerTimers_MirrorStringTimerOperations()
        {
            const int floatTimerID = 23004;
            const int intTimerID = 23005;

            IntegerFsmTimers.RemoveFloatTimer(floatTimerID);
            IntegerFsmTimers.RemoveIntTimer(intTimerID);

            try
            {
                IntegerFsmTimers.AddOrSetFloatTimer(floatTimerID, 2.5f);
                IntegerFsmTimers.AddOrSetIntTimer(intTimerID, 5);
                IntegerFsmTimers.UpdateTimers(0.5f, 2);

                Assert.That(IntegerFsmTimers.FloatTimers[floatTimerID], Is.EqualTo(2.0f).Within(0.0001f));
                Assert.That(IntegerFsmTimers.IntTimers[intTimerID], Is.EqualTo(3));

                IntegerFsmTimers.ResetFloatTimer(floatTimerID, 9f);
                IntegerFsmTimers.ResetIntTimer(intTimerID, 11);
                Assert.That(IntegerFsmTimers.FloatTimers[floatTimerID], Is.EqualTo(9f));
                Assert.That(IntegerFsmTimers.IntTimers[intTimerID], Is.EqualTo(11));
            }
            finally
            {
                IntegerFsmTimers.RemoveFloatTimer(floatTimerID);
                IntegerFsmTimers.RemoveIntTimer(intTimerID);
            }
        }

        [Test]
        public void StringAndIntegerHandles_ProduceMatchingDeferredTransitionTrace()
        {
            var stringTrace = new System.Collections.Generic.List<string>();
            var integerTrace = new System.Collections.Generic.List<string>();
            const string stringName = "ParityDeferredTransitionTrace";
            const string group = "ParityDeferredTransitionTraceTests";

            FSM_API.Create.CreateFiniteStateMachine(stringName, 0, group)
                .State("A",
                    _ => stringTrace.Add("enter:A"),
                    _ => stringTrace.Add("update:A"),
                    _ => stringTrace.Add("exit:A"))
                .State("B",
                    _ => stringTrace.Add("enter:B"),
                    _ => stringTrace.Add("update:B"),
                    _ => stringTrace.Add("exit:B"))
                .WithInitialState("A")
                .Transition("A", "B", _ => { stringTrace.Add("guard:A:B"); return true; })
                .BuildDefinition();

            var stringHandle = FSM_API.Create.CreateInstance(stringName, new Context(), group);
            stringHandle.Update(group); // The first tick enters A, updates it, and evaluates the deferred transition.
            Assert.That(stringTrace, Is.EqualTo(new[]
            {
                "enter:A",
                "update:A",
                "guard:A:B",
                "exit:A"
            }), "The string-backed handle should transition out of A on its first tick.");

            // Integer handles are explicitly initialized before the tick under test, so remove
            // only the string handle's initialization callback to compare equivalent tick traces.
            stringTrace.RemoveAt(0);

            var integerDefinition = new IntegerFsm(23008);
            integerDefinition.AddState(new IntegerFsmState(0,
                _ => integerTrace.Add("enter:A"),
                _ => integerTrace.Add("update:A"),
                _ => integerTrace.Add("exit:A")));
            integerDefinition.AddState(new IntegerFsmState(1,
                _ => integerTrace.Add("enter:B"),
                _ => integerTrace.Add("update:B"),
                _ => integerTrace.Add("exit:B")));
            integerDefinition.AddTransition(0, 1, _ => { integerTrace.Add("guard:A:B"); return true; });

            var integerHandle = new IntegerFsmHandle(integerDefinition, new Context());
            Assert.That(integerHandle.CurrentStateID, Is.EqualTo(0), "The handle must start in state A.");
            Assert.That(integerHandle.HasEnteredCurrentState, Is.False);
            integerHandle.Initialize();
            Assert.That(integerHandle.CurrentStateID, Is.EqualTo(0), "Initialization must preserve the initial state ID.");
            Assert.That(integerHandle.HasEnteredCurrentState, Is.True);
            Assert.That(integerTrace, Is.EqualTo(new[] { "enter:A" }), "Initialization should enter A only.");
            integerTrace.Clear();
            integerHandle.Update();

            // Freeze the callback trace at the exact update boundary. If some later operation
            // mutates the callback list, the immutable snapshot keeps the original result visible.
            var firstIntegerUpdateTrace = integerTrace.ToArray();
            Assert.That(firstIntegerUpdateTrace, Is.EqualTo(new[]
            {
                "update:A",
                "guard:A:B",
                "exit:A"
            }), "The first integer update should transition from A without entering B.");
            Assert.That(integerTrace, Is.EqualTo(firstIntegerUpdateTrace),
                "The integer callback trace changed after the first update completed.");
            Assert.That(stringTrace, Is.EqualTo(firstIntegerUpdateTrace));
            Assert.That(stringTrace, Is.EqualTo(new[]
            {
                "update:A",
                "guard:A:B",
                "exit:A"
            }));
            Assert.That(stringHandle.CurrentState, Is.EqualTo("B"));
            Assert.That(stringHandle.HasEnteredCurrentState, Is.False);
            Assert.That(integerHandle.CurrentStateID, Is.EqualTo(1));
            Assert.That(integerHandle.HasEnteredCurrentState, Is.False);


        }

        [Test]
        public void DirectStep_DoesNotCommitTargetRemovedByGuard()
        {
            var enteredTarget = 0;
            var definition = new IntegerFsm(23009);
            definition.AddState(new IntegerFsmState(0, null, null, null));
            definition.AddState(new IntegerFsmState(1, _ => enteredTarget++, null, null));
            definition.AddTransition(0, 1, _ =>
            {
                definition.RemoveState(1);
                return true;
            });

            var result = definition.Step(0, new Context());

            Assert.That(result, Is.EqualTo(0));
            Assert.That(definition.HasState(1), Is.False);
            Assert.That(enteredTarget, Is.Zero);
        }

        [Test]
        public void DirectStep_DoesNotEnterTargetRemovedByExitCallback()
        {
            var enteredTarget = 0;
            IntegerFsm definition = null;
            definition = new IntegerFsm(23010);
            definition.AddState(new IntegerFsmState(0, null, null, _ => definition.RemoveState(1)));
            definition.AddState(new IntegerFsmState(1, _ => enteredTarget++, null, null));
            definition.AddTransition(0, 1, _ => true);

            var result = definition.Step(0, new Context());

            Assert.That(result, Is.EqualTo(0));
            Assert.That(definition.HasState(1), Is.False);
            Assert.That(enteredTarget, Is.Zero);
        }

        [Test]
        public void Handle_DoesNotRemainEnteredWhenExitCallbackRemovesTransitionTarget()
        {
            IntegerFsm definition = null;
            definition = new IntegerFsm(23011);
            definition.AddState(new IntegerFsmState(0, null, null, _ => definition.RemoveState(1)));
            definition.AddState(new IntegerFsmState(1, null, null, null));
            definition.AddTransition(0, 1, _ => true);

            var handle = new IntegerFsmHandle(definition, new Context());
            handle.Initialize();
            handle.Update();

            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(definition.HasState(1), Is.False);
        }

    }
}
