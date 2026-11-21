using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using StringFsm = TheSingularityWorkshop.FSM_API.FSM;
using StringFsmHandle = TheSingularityWorkshop.FSM_API.FSMHandle;
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
        public void NormalTransition_EntryCallbackTiming_MatchesStringBackedImplementation()
        {
            var stringTrace = new List<string>();
            var integerTrace = new List<string>();
            var stringContext = new LifecycleContext(stringTrace);
            var integerContext = new LifecycleContext(integerTrace);

            var stringDefinition = new StringFsm { Name = "StringLifecycleParity" };
            stringDefinition.AddState(new TheSingularityWorkshop.FSM_API.FSMState(
                "A", _ => stringTrace.Add("A.Enter"), _ => stringTrace.Add("A.Update"), _ => stringTrace.Add("A.Exit")));
            stringDefinition.AddState(new TheSingularityWorkshop.FSM_API.FSMState(
                "B", _ => stringTrace.Add("B.Enter"), _ => stringTrace.Add("B.Update"), _ => stringTrace.Add("B.Exit")));
            stringDefinition.AddTransition("A", "B", context => ((LifecycleContext)context).ShouldTransition);

            var integerDefinition = new IntegerFsm(23006);
            integerDefinition.AddState(new IntegerFsmState(
                0, _ => integerTrace.Add("A.Enter"), _ => integerTrace.Add("A.Update"), _ => integerTrace.Add("A.Exit")));
            integerDefinition.AddState(new IntegerFsmState(
                1, _ => integerTrace.Add("B.Enter"), _ => integerTrace.Add("B.Update"), _ => integerTrace.Add("B.Exit")));
            integerDefinition.AddTransition(0, 1, context => ((LifecycleContext)context).ShouldTransition);

            var stringHandle = new StringFsmHandle(stringDefinition, stringContext);
            var integerHandle = new IntegerFsmHandle(integerDefinition, integerContext);
            integerHandle.Initialize();

            stringHandle.Update();
            integerHandle.Update();
            Assert.That(stringTrace, Is.EqualTo(integerTrace));
            Assert.That(stringHandle.CurrentState, Is.EqualTo("A"));
            Assert.That(integerHandle.CurrentStateID, Is.EqualTo(0));

            stringContext.ShouldTransition = true;
            integerContext.ShouldTransition = true;
            stringHandle.Update();
            integerHandle.Update();
            Assert.That(stringTrace, Is.EqualTo(integerTrace));
            Assert.That(stringHandle.CurrentState, Is.EqualTo("B"));
            Assert.That(integerHandle.CurrentStateID, Is.EqualTo(1));
            Assert.That(stringTrace, Does.Not.Contain("B.Enter"));

            stringHandle.Update();
            integerHandle.Update();
            Assert.That(stringTrace, Is.EqualTo(integerTrace));
            Assert.That(stringTrace, Does.Contain("B.Enter"));
        }

        private sealed class LifecycleContext : IStateContext
        {
            public LifecycleContext(List<string> trace)
            {
                Trace = trace;
            }

            public List<string> Trace { get; }
            public bool ShouldTransition { get; set; }
            public bool AnyStateShouldTransition { get; set; }
            public bool IsValid { get; set; } = true;
            public string Name { get; set; } = "LifecycleParity";
            public int Context_ID => 23006;
            public bool HasEnteredCurrentState { get; set; }
        }


        [Test]
        public void ContextInvalidationAfterEntry_SuspendsAndResumesWithoutReentry_InBothImplementations()
        {
            var stringTrace = new List<string>();
            var integerTrace = new List<string>();
            var stringContext = new LifecycleContext(stringTrace);
            var integerContext = new LifecycleContext(integerTrace);

            var stringDefinition = new StringFsm { Name = "StringContextResumeParity" };
            stringDefinition.AddState(new TheSingularityWorkshop.FSM_API.FSMState(
                "A", _ => stringTrace.Add("A.Enter"), _ => stringTrace.Add("A.Update"), _ => stringTrace.Add("A.Exit")));

            var integerDefinition = new IntegerFsm(23017, -1);
            integerDefinition.AddState(new IntegerFsmState(
                0, _ => integerTrace.Add("A.Enter"), _ => integerTrace.Add("A.Update"), _ => integerTrace.Add("A.Exit")));

            var stringHandle = new StringFsmHandle(stringDefinition, stringContext);
            var runtime = new FSMRuntime();
            runtime.Register(integerDefinition, 18);
            var integerHandle = runtime.CreateInstance(23017, integerContext);

            stringHandle.Update();
            runtime.Update(18);
            Assert.That(stringTrace, Is.EqualTo(new[] { "A.Enter", "A.Update" }));
            Assert.That(integerTrace, Is.EqualTo(stringTrace));
            Assert.That(stringHandle.HasEnteredCurrentState, Is.True);
            Assert.That(integerHandle.HasEnteredCurrentState, Is.True);

            stringContext.IsValid = false;
            integerContext.IsValid = false;
            stringHandle.Update();
            runtime.Update(18);

            Assert.That(stringTrace, Is.EqualTo(new[] { "A.Enter", "A.Update" }),
                "An invalid context must suspend callbacks without changing lifecycle state.");
            Assert.That(integerTrace, Is.EqualTo(stringTrace));
            Assert.That(stringHandle.IsDestroyed, Is.False);
            Assert.That(integerHandle.IsDestroyed, Is.False);
            Assert.That(runtime.GetInstances(23017), Does.Contain(integerHandle));
            Assert.That(stringHandle.HasEnteredCurrentState, Is.True);
            Assert.That(integerHandle.HasEnteredCurrentState, Is.True);

            stringContext.IsValid = true;
            integerContext.IsValid = true;
            stringHandle.Update();
            runtime.Update(18);

            Assert.That(stringTrace, Is.EqualTo(new[] { "A.Enter", "A.Update", "A.Update" }));
            Assert.That(integerTrace, Is.EqualTo(stringTrace),
                "Resuming a valid context must continue the existing state without replaying Enter.");
            Assert.That(stringHandle.IsDestroyed, Is.False);
            Assert.That(integerHandle.IsDestroyed, Is.False);
        }

        [Test]
        public void AnyStateTransition_PriorityAndCallbackTrace_MatchStringBackedImplementation()
        {
            var stringTrace = new List<string>();
            var integerTrace = new List<string>();
            var stringContext = new LifecycleContext(stringTrace) { ShouldTransition = true, AnyStateShouldTransition = true };
            var integerContext = new LifecycleContext(integerTrace) { ShouldTransition = true, AnyStateShouldTransition = true };

            var stringDefinition = new StringFsm { Name = "StringAnyStateParity" };
            stringDefinition.AddState(new TheSingularityWorkshop.FSM_API.FSMState(
                "A", _ => stringTrace.Add("A.Enter"), _ => stringTrace.Add("A.Update"), _ => stringTrace.Add("A.Exit")));
            stringDefinition.AddState(new TheSingularityWorkshop.FSM_API.FSMState(
                "B", _ => stringTrace.Add("B.Enter"), _ => stringTrace.Add("B.Update"), _ => stringTrace.Add("B.Exit")));
            stringDefinition.AddState(new TheSingularityWorkshop.FSM_API.FSMState(
                "C", _ => stringTrace.Add("C.Enter"), _ => stringTrace.Add("C.Update"), _ => stringTrace.Add("C.Exit")));
            stringDefinition.AddTransition("A", "B", context => ((LifecycleContext)context).ShouldTransition);
            stringDefinition.AddAnyStateTransition("C", context => ((LifecycleContext)context).AnyStateShouldTransition);

            var integerDefinition = new IntegerFsm(23007);
            integerDefinition.AddState(new IntegerFsmState(
                0, _ => integerTrace.Add("A.Enter"), _ => integerTrace.Add("A.Update"), _ => integerTrace.Add("A.Exit")));
            integerDefinition.AddState(new IntegerFsmState(
                1, _ => integerTrace.Add("B.Enter"), _ => integerTrace.Add("B.Update"), _ => integerTrace.Add("B.Exit")));
            integerDefinition.AddState(new IntegerFsmState(
                2, _ => integerTrace.Add("C.Enter"), _ => integerTrace.Add("C.Update"), _ => integerTrace.Add("C.Exit")));
            integerDefinition.AddTransition(0, 1, context => ((LifecycleContext)context).ShouldTransition);
            integerDefinition.AddAnyStateTransition(2, context => ((LifecycleContext)context).AnyStateShouldTransition);

            var stringHandle = new StringFsmHandle(stringDefinition, stringContext);
            var integerHandle = new IntegerFsmHandle(integerDefinition, integerContext);
            integerHandle.Initialize();

            stringHandle.Update();
            integerHandle.Update();

            Assert.That(stringTrace, Is.EqualTo(integerTrace));
            Assert.That(stringHandle.CurrentState, Is.EqualTo("C"));
            Assert.That(integerHandle.CurrentStateID, Is.EqualTo(2));
            Assert.That(stringTrace, Does.Not.Contain("C.Enter"));
        }

        [Test]
        public void InvalidContext_SuspendsInitializationAndUpdate_InBothImplementations()
        {
            var stringTrace = new List<string>();
            var integerTrace = new List<string>();
            var stringContext = new LifecycleContext(stringTrace);
            var integerContext = new LifecycleContext(integerTrace);

            var stringDefinition = new StringFsm { Name = "StringInvalidContextParity" };
            stringDefinition.AddState(new TheSingularityWorkshop.FSM_API.FSMState(
                "A", _ => stringTrace.Add("A.Enter"), _ => stringTrace.Add("A.Update"), null));

            var integerDefinition = new IntegerFsm(23008);
            integerDefinition.AddState(new IntegerFsmState(
                0, _ => integerTrace.Add("A.Enter"), _ => integerTrace.Add("A.Update"), null));

            var stringHandle = new StringFsmHandle(stringDefinition, stringContext);
            var integerHandle = new IntegerFsmHandle(integerDefinition, integerContext);
            // Handle construction initializes context validity. Suspend both handles
            // afterward so this test exercises the invalid-context execution gate.
            stringContext.IsValid = false;
            integerContext.IsValid = false;
            integerHandle.Initialize();
            stringHandle.Update();
            integerHandle.Update();

            Assert.That(stringTrace, Is.Empty);
            Assert.That(integerTrace, Is.Empty);
            Assert.That(integerHandle.HasEnteredCurrentState, Is.False);

            stringContext.IsValid = true;
            integerContext.IsValid = true;
            stringHandle.Update();
            integerHandle.Update();

            Assert.That(stringTrace, Is.EqualTo(integerTrace));
            Assert.That(integerTrace, Is.EqualTo(new[] { "A.Enter", "A.Update" }));
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
        public void RuntimeReregisteringDefinition_RebindsLiveInstancesWithoutDestroyingThem()
        {
            var trace = new List<string>();
            var context = new LifecycleContext(trace);
            var runtime = new FSMRuntime();

            var originalDefinition = new IntegerFsm(23014, -1);
            originalDefinition.AddState(new IntegerFsmState(
                0,
                _ => trace.Add("original.enter"),
                _ => trace.Add("original.update"),
                _ => trace.Add("original.exit")));

            runtime.Register(originalDefinition, 14);
            var handle = runtime.CreateInstance(23014, context);

            runtime.Update(14);
            Assert.That(trace, Is.EqualTo(new[] { "original.enter", "original.update" }));
            Assert.That(handle.HasEnteredCurrentState, Is.True);

            var replacementDefinition = new IntegerFsm(23014, -1);
            replacementDefinition.AddState(new IntegerFsmState(
                0,
                _ => trace.Add("replacement.enter"),
                _ => trace.Add("replacement.update"),
                _ => trace.Add("replacement.exit")));

            runtime.Register(replacementDefinition, 14);

            Assert.That(handle.IsDestroyed, Is.False, "Replacing a definition must not destroy its live handle.");
            Assert.That(handle.Context, Is.SameAs(context), "The caller-owned context reference must be preserved.");
            Assert.That(handle.Definition, Is.SameAs(replacementDefinition));
            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(runtime.GetInstances(23014), Does.Contain(handle));

            runtime.Update(14);

            Assert.That(trace, Is.EqualTo(new[]
            {
                "original.enter",
                "original.update",
                "replacement.update"
            }), "Subsequent updates must use the replacement definition without replaying entry or exit.");
            Assert.That(handle.IsDestroyed, Is.False);
            Assert.That(context.IsValid, Is.True);
        }

        [Test]
        public void RuntimeDefinitionReplacement_MissingCurrentStateRecoversAndEntersExactlyOnce()
        {
            var trace = new List<string>();
            var context = new LifecycleContext(trace);
            var runtime = new FSMRuntime();

            var originalDefinition = new IntegerFsm(23015, -1);
            originalDefinition.AddState(new IntegerFsmState(
                0,
                _ => trace.Add("original.enter"),
                _ => trace.Add("original.update"),
                _ => trace.Add("original.exit")));

            runtime.Register(originalDefinition, 15);
            var handle = runtime.CreateInstance(23015, context);
            runtime.Update(15);

            Assert.That(handle.CurrentStateID, Is.EqualTo(0));
            Assert.That(handle.HasEnteredCurrentState, Is.True);

            var replacementDefinition = new IntegerFsm(23015, -1);
            replacementDefinition.AddState(new IntegerFsmState(
                5,
                _ => trace.Add("replacement.enter"),
                _ => trace.Add("replacement.update"),
                _ => trace.Add("replacement.exit")));

            runtime.Register(replacementDefinition, 15);
            runtime.Update(15);

            Assert.That(handle.CurrentStateID, Is.EqualTo(5));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(handle.IsDestroyed, Is.False);
            Assert.That(context.IsValid, Is.True);
            Assert.That(trace, Is.EqualTo(new[]
            {
                "original.enter",
                "original.update"
            }), "Recovery should not invoke callbacks for a state absent from the replacement.");

            runtime.Update(15);

            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(trace, Is.EqualTo(new[]
            {
                "original.enter",
                "original.update",
                "replacement.enter",
                "replacement.update"
            }), "The replacement initial state must enter exactly once before its first update.");
        }


        [Test]
        public void RuntimeDefinitionReplacement_MovesLiveInstancesToReplacementProcessingGroup()
        {
            var trace = new List<string>();
            var context = new LifecycleContext(trace);
            var runtime = new FSMRuntime();

            var originalDefinition = new IntegerFsm(23016, -1);
            originalDefinition.AddState(new IntegerFsmState(
                0,
                _ => trace.Add("original.enter"),
                _ => trace.Add("original.update"),
                null));

            runtime.Register(originalDefinition, 16);
            var handle = runtime.CreateInstance(23016, context);
            runtime.Update(16);

            Assert.That(trace, Is.EqualTo(new[] { "original.enter", "original.update" }));
            Assert.That(runtime.GetHandleCount(16), Is.EqualTo(1));

            var replacementDefinition = new IntegerFsm(23016, -1);
            replacementDefinition.AddState(new IntegerFsmState(
                0,
                _ => trace.Add("replacement.enter"),
                _ => trace.Add("replacement.update"),
                null));

            runtime.Register(replacementDefinition, 17);

            Assert.That(handle.Definition, Is.SameAs(replacementDefinition));
            Assert.That(runtime.GetHandleCount(16), Is.Zero);
            Assert.That(runtime.GetHandleCount(17), Is.EqualTo(1));

            runtime.Update(16);
            Assert.That(trace, Is.EqualTo(new[] { "original.enter", "original.update" }),
                "The old processing group must no longer schedule the rebound handle.");

            runtime.Update(17);
            Assert.That(trace, Is.EqualTo(new[]
            {
                "original.enter",
                "original.update",
                "replacement.update"
            }), "The replacement group must schedule the existing handle without replaying entry.");
            Assert.That(handle.IsDestroyed, Is.False);
            Assert.That(handle.Context, Is.SameAs(context));
        }


        [TestCase(-1, 4)]
        [TestCase(0, 0)]
        [TestCase(2, 2)]
        public void RuntimeScheduler_ProcessRateCadence_MatchesStringBackedImplementation(int processRate, int expectedUpdates)
        {
            FsmApi.Internal.ResetAPI(true);

            var stringTrace = new List<string>();
            var integerTrace = new List<string>();
            var stringContext = new LifecycleContext(stringTrace);
            var integerContext = new LifecycleContext(integerTrace);

            const string processingGroup = "SchedulerParity";
            const int processingGroupID = 23;
            const string fsmName = "StringSchedulerParity";
            const int fsmID = 23009;

            FsmApi.Create.CreateFiniteStateMachine(fsmName, processRate, processingGroup)
                .State("A", null, _ => stringTrace.Add("Update"), null)
                .BuildDefinition();
            var stringHandle = FsmApi.Create.CreateInstance(fsmName, stringContext, processingGroup);

            var integerDefinition = new IntegerFsm(fsmID, processRate);
            integerDefinition.AddState(new IntegerFsmState(0, null, _ => integerTrace.Add("Update"), null));
            var integerRuntime = new FSMRuntime();
            integerRuntime.Register(integerDefinition, processingGroupID);
            var integerHandle = integerRuntime.CreateInstance(fsmID, integerContext);

            for (var tick = 0; tick < 4; tick++)
            {
                FsmApi.Interaction.Update(processingGroup);
                integerRuntime.Update(processingGroupID);
            }

            Assert.That(stringTrace.Count, Is.EqualTo(expectedUpdates), "String-backed update cadence changed.");
            Assert.That(integerTrace.Count, Is.EqualTo(expectedUpdates), "Integer-backed update cadence must match.");
            Assert.That(integerTrace, Is.EqualTo(stringTrace));
            Assert.That(stringHandle.IsDestroyed, Is.False);
            Assert.That(integerHandle.IsDestroyed, Is.False);
        }

        [Test]
        public void RuntimeScheduler_ProcessingGroupIsolation_MatchesStringBackedImplementation()
        {
            FsmApi.Internal.ResetAPI(true);

            var stringGroupAUpdates = 0;
            var stringGroupBUpdates = 0;
            var integerGroupAUpdates = 0;
            var integerGroupBUpdates = 0;

            FsmApi.Create.CreateFiniteStateMachine("StringGroupA", -1, "GroupA")
                .State("A", null, _ => stringGroupAUpdates++, null).BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine("StringGroupB", -1, "GroupB")
                .State("A", null, _ => stringGroupBUpdates++, null).BuildDefinition();
            FsmApi.Create.CreateInstance("StringGroupA", new LifecycleContext(new List<string>()), "GroupA");
            FsmApi.Create.CreateInstance("StringGroupB", new LifecycleContext(new List<string>()), "GroupB");

            var integerRuntime = new FSMRuntime();
            var integerA = new IntegerFsm(23010, -1);
            integerA.AddState(new IntegerFsmState(0, null, _ => integerGroupAUpdates++, null));
            var integerB = new IntegerFsm(23011, -1);
            integerB.AddState(new IntegerFsmState(0, null, _ => integerGroupBUpdates++, null));
            integerRuntime.Register(integerA, 10);
            integerRuntime.Register(integerB, 11);
            integerRuntime.CreateInstance(23010, new LifecycleContext(new List<string>()));
            integerRuntime.CreateInstance(23011, new LifecycleContext(new List<string>()));

            FsmApi.Interaction.Update("GroupA");
            integerRuntime.Update(10);
            FsmApi.Interaction.Update("GroupB");
            integerRuntime.Update(11);

            Assert.That(stringGroupAUpdates, Is.EqualTo(1));
            Assert.That(integerGroupAUpdates, Is.EqualTo(stringGroupAUpdates));
            Assert.That(stringGroupBUpdates, Is.EqualTo(1));
            Assert.That(integerGroupBUpdates, Is.EqualTo(stringGroupBUpdates));
        }

        [Test]
        public void RuntimeScheduler_IndependentDefinitionCounters_MatchStringBackedImplementation()
        {
            FsmApi.Internal.ResetAPI(true);

            var stringRateTwo = 0;
            var stringRateThree = 0;
            var integerRateTwo = 0;
            var integerRateThree = 0;

            FsmApi.Create.CreateFiniteStateMachine("StringRateTwo", 2, "IndependentCounters")
                .State("A", null, _ => stringRateTwo++, null).BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine("StringRateThree", 3, "IndependentCounters")
                .State("A", null, _ => stringRateThree++, null).BuildDefinition();
            FsmApi.Create.CreateInstance("StringRateTwo", new LifecycleContext(new List<string>()), "IndependentCounters");
            FsmApi.Create.CreateInstance("StringRateThree", new LifecycleContext(new List<string>()), "IndependentCounters");

            var integerRuntime = new FSMRuntime();
            var integerRateTwoDefinition = new IntegerFsm(23012, 2);
            integerRateTwoDefinition.AddState(new IntegerFsmState(0, null, _ => integerRateTwo++, null));
            var integerRateThreeDefinition = new IntegerFsm(23013, 3);
            integerRateThreeDefinition.AddState(new IntegerFsmState(0, null, _ => integerRateThree++, null));
            integerRuntime.Register(integerRateTwoDefinition, 12);
            integerRuntime.Register(integerRateThreeDefinition, 12);
            integerRuntime.CreateInstance(23012, new LifecycleContext(new List<string>()));
            integerRuntime.CreateInstance(23013, new LifecycleContext(new List<string>()));

            for (var tick = 0; tick < 6; tick++)
            {
                FsmApi.Interaction.Update("IndependentCounters");
                integerRuntime.Update(12);
            }

            Assert.That(stringRateTwo, Is.EqualTo(3));
            Assert.That(integerRateTwo, Is.EqualTo(stringRateTwo));
            Assert.That(stringRateThree, Is.EqualTo(2));
            Assert.That(integerRateThree, Is.EqualTo(stringRateThree));
        }


        [Test]
        public void ErrorThresholdTeardown_MatchesStringBackedImplementation()
        {
            FsmApi.Internal.ResetAPI(true);
            FsmApi.Error.Reset();
            FsmApi.Error.InstanceErrorThreshold = 2;
            FsmApi.Error.DefinitionErrorThreshold = 1;

            const string stringName = "StringErrorParity";
            const string stringGroup = "StringErrorParityGroup";
            const int integerID = 23091;
            const int integerGroupID = 91;

            FsmApi.Create.CreateFiniteStateMachine(stringName, -1, stringGroup)
                .State("A", null, _ => throw new InvalidOperationException("string callback failure"), null)
                .WithInitialState("A")
                .BuildDefinition();
            var stringHandle = FsmApi.Create.CreateInstance(stringName, new Context(), stringGroup);

            var integerRuntime = new FSMRuntime();
            var integerDefinition = new FSMBuilderInt(integerID, processRate: -1, processingGroupID: integerGroupID)
                .State(0, null, _ => throw new InvalidOperationException("integer callback failure"), null)
                .WithInitialState(0)
                .BuildDefinition();
            integerRuntime.Register(integerDefinition, integerGroupID);
            var integerHandle = integerRuntime.CreateInstance(integerID, new Context());

            // First failure increments both counters but remains below the instance threshold.
            FsmApi.Interaction.Update(stringGroup);
            integerRuntime.Update(integerGroupID);

            Assert.That(FsmApi.Interaction.Exists(stringName, stringGroup), Is.True);
            Assert.That(FsmApi.Interaction.GetInstances(stringName, stringGroup).Count, Is.EqualTo(1));
            Assert.That(FsmApi.Error.GetErrorCounts().ContainsKey(stringHandle), Is.True);
            Assert.That(integerRuntime.Contains(integerID), Is.True);
            Assert.That(integerRuntime.GetInstances(integerID).Count, Is.EqualTo(1));
            Assert.That(integerRuntime.GetInstanceErrorCounts().ContainsKey(integerHandle), Is.True);
            Assert.That(integerRuntime.GetDefinitionErrorCounts().ContainsKey(integerID), Is.False);

            // The second failure reaches the same instance threshold in both runtimes.
            // Each runtime removes the failed handle and, with definition threshold 1,
            // unregisters its definition at the end of that tick.
            FsmApi.Interaction.Update(stringGroup);
            integerRuntime.Update(integerGroupID);

            Assert.That(stringHandle.IsDestroyed, Is.True);
            Assert.That(integerHandle.IsDestroyed, Is.True);
            Assert.That(FsmApi.Interaction.Exists(stringName, stringGroup), Is.False);
            Assert.That(FsmApi.Interaction.GetInstances(stringName, stringGroup).Count, Is.Zero);
            Assert.That(integerRuntime.Contains(integerID), Is.False);
            Assert.That(integerRuntime.GetInstances(integerID).Count, Is.Zero);
            Assert.That(FsmApi.Error.GetErrorCounts().Count, Is.Zero);
            Assert.That(FsmApi.Error.GetDefinitionErrorCounts().Count, Is.Zero);
            Assert.That(integerRuntime.GetInstanceErrorCounts().Count, Is.Zero);
            Assert.That(integerRuntime.GetDefinitionErrorCounts().Count, Is.Zero);

            FsmApi.Error.Reset();
            FsmApi.Internal.ResetAPI(true);
        }


    }
}
