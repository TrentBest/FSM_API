using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FSM_API.Internal;
using TheSingularityWorkshop.FSM_API.Tests;


namespace TheSingularityWorkshop.FSM_API.Tests.Interaction
{
    /// <summary>
    /// A simple mock implementation of IStateContext for our tests.
    /// </summary>
    public class TestContext : IStateContext
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = "TestContext";

        /// <summary>
        /// 
        /// </summary>
        public int OnEnterCounter { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public int OnUpdateCounter { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public int OnExitCounter { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public bool ShouldTransition { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public bool HasEntered { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public bool AnyStateShouldTransition { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public bool HasEnteredCurrentState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TestData { get; internal set; }
    }


    /// <summary>
    /// State Action Methods for the FSM
    /// </summary>
    public static class TestStateActions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static void OnEnterStateA(IStateContext context)
        {
            if (context is TestContext ctx)
            {
                ctx.OnEnterCounter++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static void OnUpdateStateA(IStateContext context)
        {
            if (context is TestContext ctx)
            {
                ctx.OnUpdateCounter++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static void OnExitStateA(IStateContext context)
        {
            if (context is TestContext ctx)
            {
                ctx.OnExitCounter++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool ShouldTransition(IStateContext context)
        {
            if (context is TestContext ctx)
            {
                return ctx.ShouldTransition;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool ShouldAnyStateTransition(IStateContext context)
        {
            if (context is TestContext ctx)
            {
                return ctx.AnyStateShouldTransition;
            }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Interaction_Update_Tests
    {
        private const string _testProcessingGroup = "TestGroup";
        private const string _testFsmName = "TestFSM";

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Reset the API state before each test to ensure test isolation.
            // This is a crucial step for unit testing static APIs.
            FSM_API.Internal.ResetAPI();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Update_ShouldHandleMultipleIndependentInstances()
        {
            // Arrange
            FSM_API.Create.CreateProcessingGroup(_testProcessingGroup);
            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
                .State("StateA", TestStateActions.OnEnterStateA, TestStateActions.OnUpdateStateA, TestStateActions.OnExitStateA)
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", TestStateActions.ShouldTransition)
                .WithInitialState("StateA")
                .BuildDefinition();

            // Create two completely separate contexts and handles
            var ctx1 = new TestContext();
            var handle1 = FSM_API.Create.CreateInstance(_testFsmName, ctx1, _testProcessingGroup);

            var ctx2 = new TestContext();
            var handle2 = FSM_API.Create.CreateInstance(_testFsmName, ctx2, _testProcessingGroup);

            // Act 1: Initial update
            FSM_API.Interaction.Update(_testProcessingGroup);

            // Assertions for both instances after the first update
            // Both should have entered StateA once and updated once.
            Assert.That(handle1.CurrentState, Is.EqualTo("StateA"));
            Assert.That(ctx1.OnEnterCounter, Is.EqualTo(1));
            Assert.That(ctx1.OnUpdateCounter, Is.EqualTo(1));
            Assert.That(ctx1.OnExitCounter, Is.EqualTo(0));

            Assert.That(handle2.CurrentState, Is.EqualTo("StateA"));
            Assert.That(ctx2.OnEnterCounter, Is.EqualTo(1));
            Assert.That(ctx2.OnUpdateCounter, Is.EqualTo(1));
            Assert.That(ctx2.OnExitCounter, Is.EqualTo(0));

            // Act 2: Trigger transition on only one instance
            ctx1.ShouldTransition = true;
            FSM_API.Interaction.Update(_testProcessingGroup);

            // Assertions after the second update
            // Instance 1 should have transitioned, while Instance 2 should have not.
            Assert.That(handle1.CurrentState, Is.EqualTo("StateB"));
            Assert.That(ctx1.OnEnterCounter, Is.EqualTo(1));
            Assert.That(ctx1.OnUpdateCounter, Is.EqualTo(2));
            Assert.That(ctx1.OnExitCounter, Is.EqualTo(1));

            Assert.That(handle2.CurrentState, Is.EqualTo("StateA"));
            Assert.That(ctx2.OnEnterCounter, Is.EqualTo(1));
            Assert.That(ctx2.OnUpdateCounter, Is.EqualTo(2));
            Assert.That(ctx2.OnExitCounter, Is.EqualTo(0));
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void Update_ShouldPrioritizeAnyStateTransition()
        //{
        //    // Arrange
        //    FSM_API.Create.CreateProcessingGroup(_testProcessingGroup);
        //    FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
        //        .State("StateA", TestStateActions.OnEnterStateA, TestStateActions.OnUpdateStateA, TestStateActions.OnExitStateA)
        //        .State("StateB", null, null, null) // Target for regular transition
        //        .State("StateC", null, null, null) // Target for any-state transition
        //                                           // Regular transition from StateA to StateB
        //        .Transition("StateA", "StateB", TestStateActions.ShouldTransition)
        //        // Any-state transition from anywhere to StateC
        //        .AnyTransition("StateC", TestStateActions.ShouldAnyStateTransition)
        //        .WithInitialState("StateA")
        //        .BuildDefinition();

        //    var ctx = new TestContext();
        //    var handle = FSM_API.Create.CreateInstance(_testFsmName, ctx, _testProcessingGroup);

        //    // Initial update to put it in StateA and run OnEnter/OnUpdate
        //    FSM_API.Interaction.Update(_testProcessingGroup);
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(0));

        //    // Act: Trigger both the regular transition condition AND the Any-State transition condition
        //    ctx.ShouldTransition = true; // Condition for StateA -> StateB
        //    ctx.AnyStateShouldTransition = true; // Condition for Any State -> StateC

        //    FSM_API.Interaction.Update(_testProcessingGroup);

        //    // Assert: Verify that the Any-State transition took priority
        //    // FSM should have exited StateA and entered StateC
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateC"));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(1)); // StateA exited
        //                                                   // OnEnter for StateC is null, so OnEnterCounter should not increase.
        //                                                   // OnUpdate for StateA ran one last time before transition.
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(1));
        //}

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Update_Succeeds()
        {
            FSM_API.Create.CreateProcessingGroup(_testProcessingGroup);
            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
                .State("StateA", TestStateActions.OnEnterStateA, TestStateActions.OnUpdateStateA, TestStateActions.OnExitStateA)
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", TestStateActions.ShouldTransition)
                .WithInitialState("StateA")
                .BuildDefinition();

            var handle = FSM_API.Create.CreateInstance(_testFsmName, new TestContext(), _testProcessingGroup);

            Assert.That(handle.HasEnteredCurrentState, Is.False, "FSM should not have entered the initial state before the first update.");
            FSM_API.Interaction.Update(_testProcessingGroup);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"), "FSM should be in StateA after the first update.");
            Assert.That(handle.HasEnteredCurrentState, Is.True, "FSM should have entered StateA after the first update.");
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void Update_ShouldHandleMultipleIndependentInstances()
        //{
        //    // Arrange
        //    FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
        //        .State("StateA", TestStateActions.OnEnterStateA, TestStateActions.OnUpdateStateA, TestStateActions.OnExitStateA)
        //        .State("StateB", null, null, null)
        //        .Transition("StateA", "StateB", TestStateActions.ShouldTransition)
        //        .WithInitialState("StateA")
        //        .BuildDefinition();

        //    // Create two completely separate contexts and handles
        //    var ctx1 = new TestContext();
        //    var handle1 = FSM_API.Create.CreateInstance(_testFsmName, ctx1, _testProcessingGroup);

        //    var ctx2 = new TestContext();
        //    var handle2 = FSM_API.Create.CreateInstance(_testFsmName, ctx2, _testProcessingGroup);

        //    // Act 1: Initial update
        //    FSM_API.Interaction.Update(_testProcessingGroup);

        //    // Assertions for both instances after the first update
        //    // Both should have entered StateA once and updated once.
        //    Assert.That(handle1.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx1.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx1.OnUpdateCounter, Is.EqualTo(1));
        //    Assert.That(ctx1.OnExitCounter, Is.EqualTo(0));

        //    Assert.That(handle2.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx2.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx2.OnUpdateCounter, Is.EqualTo(1));
        //    Assert.That(ctx2.OnExitCounter, Is.EqualTo(0));

        //    // Act 2: Trigger transition on only one instance
        //    ctx1.ShouldTransition = true;
        //    FSM_API.Interaction.Update(_testProcessingGroup);

        //    // Assertions after the second update
        //    // Instance 1 should have transitioned, while Instance 2 should have not.
        //    Assert.That(handle1.CurrentState, Is.EqualTo("StateB"));
        //    Assert.That(ctx1.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx1.OnUpdateCounter, Is.EqualTo(2));
        //    Assert.That(ctx1.OnExitCounter, Is.EqualTo(1));

        //    Assert.That(handle2.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx2.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx2.OnUpdateCounter, Is.EqualTo(2));
        //    Assert.That(ctx2.OnExitCounter, Is.EqualTo(0));
        //}


        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void Update_ShouldPrioritizeAnyStateTransition()
        //{
        //    // Arrange
        //    FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
        //        .State("StateA", TestStateActions.OnEnterStateA, TestStateActions.OnUpdateStateA, TestStateActions.OnExitStateA)
        //        .State("StateB", null, null, null) // Target for regular transition
        //        .State("StateC", null, null, null) // Target for any-state transition
        //                                           // Regular transition from StateA to StateB
        //        .Transition("StateA", "StateB", TestStateActions.ShouldTransition)
        //        // Any-state transition from anywhere to StateC
        //        .AnyTransition("StateC", TestStateActions.ShouldAnyStateTransition)
        //        .WithInitialState("StateA")
        //        .BuildDefinition();

        //    var ctx = new TestContext();
        //    var handle = FSM_API.Create.CreateInstance(_testFsmName, ctx, _testProcessingGroup);

        //    // Initial update to put it in StateA and run OnEnter/OnUpdate
        //    FSM_API.Interaction.Update(_testProcessingGroup);
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(0));

        //    // Act: Trigger both the regular transition condition AND the Any-State transition condition
        //    ctx.ShouldTransition = true; // Condition for StateA -> StateB
        //    ctx.AnyStateShouldTransition = true; // Condition for Any State -> StateC

        //    FSM_API.Interaction.Update(_testProcessingGroup);

        //    // Assert: Verify that the Any-State transition took priority
        //    // FSM should have exited StateA and entered StateC
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateC"));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(1)); // StateA exited
        //    // OnEnter for StateC is null, so OnEnterCounter should not increase.
        //    // OnUpdate for StateA ran one last time before transition.
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(1));
        //}

        [Test]
        public void InvalidContext_SuspendsRegisteredHandle_AndResumesSameInstance()
        {
            var enterCount = 0;
            var updateCount = 0;
            var guardCount = 0;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
                .State("StateA", _ => enterCount++, _ => updateCount++, null)
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", _ => { guardCount++; return false; })
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext { IsValid = false };
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            // Successful handle construction initializes the caller-owned execution gate.
            Assert.That(context.IsValid, Is.True);

            FSM_API.Interaction.Update(_testProcessingGroup);
            Assert.That(enterCount, Is.EqualTo(1));
            Assert.That(updateCount, Is.EqualTo(1));
            Assert.That(guardCount, Is.EqualTo(1));

            context.IsValid = false;
            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(enterCount, Is.EqualTo(1));
            Assert.That(updateCount, Is.EqualTo(1));
            Assert.That(guardCount, Is.EqualTo(1));
            Assert.That(handle.IsDestroyed, Is.False);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(handle.Context, Is.SameAs(context));

            context.IsValid = true;
            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(enterCount, Is.EqualTo(1));
            Assert.That(updateCount, Is.EqualTo(2));
            Assert.That(guardCount, Is.EqualTo(2));
            Assert.That(handle.IsDestroyed, Is.False);
        }

        [Test]
        public void InvalidatingDuringOnUpdate_StopsTransitionEvaluationForThatTick()
        {
            var updateCount = 0;
            var guardCount = 0;
            var exitCount = 0;
            var suspendOnce = true;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
                .State("StateA", null, context =>
                {
                    updateCount++;
                    if (suspendOnce)
                    {
                        suspendOnce = false;
                        context.IsValid = false;
                    }
                }, _ => exitCount++)
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", _ => { guardCount++; return true; })
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext();
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(context.IsValid, Is.False);
            Assert.That(updateCount, Is.EqualTo(1));
            Assert.That(guardCount, Is.Zero);
            Assert.That(exitCount, Is.Zero);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(handle.IsDestroyed, Is.False);

            context.IsValid = true;
            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(updateCount, Is.EqualTo(2));
            Assert.That(guardCount, Is.EqualTo(1));
            Assert.That(exitCount, Is.EqualTo(1));
            Assert.That(handle.CurrentState, Is.EqualTo("StateB"));
        }

        [Test]
        public void InvalidatingDuringTransitionGuard_PreventsExitAndTransitionCommit()
        {
            var guardCount = 0;
            var exitCount = 0;
            var suspendOnce = true;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
                .State("StateA", null, null, _ => exitCount++)
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", context =>
                {
                    guardCount++;
                    if (suspendOnce)
                    {
                        suspendOnce = false;
                        context.IsValid = false;
                        return true;
                    }
                    return false;
                })
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext();
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(guardCount, Is.EqualTo(1));
            Assert.That(exitCount, Is.Zero);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(handle.IsDestroyed, Is.False);

            context.IsValid = true;
            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(guardCount, Is.EqualTo(2));
            Assert.That(exitCount, Is.Zero);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        }

        [Test]
        public void InvalidatingDuringOnExit_DoesNotCommitTransition_AndReentersCurrentStateOnResume()
        {
            var enterCount = 0;
            var exitCount = 0;
            var guardCount = 0;
            var suspendOnce = true;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
                .State("StateA", _ => enterCount++, null, context =>
                {
                    exitCount++;
                    if (suspendOnce)
                    {
                        suspendOnce = false;
                        context.IsValid = false;
                    }
                })
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", _ => { guardCount++; return true; })
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext();
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(context.IsValid, Is.False);
            Assert.That(exitCount, Is.EqualTo(1));
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(handle.IsDestroyed, Is.False);

            context.IsValid = true;
            FSM_API.Interaction.Update(_testProcessingGroup);

            Assert.That(enterCount, Is.EqualTo(2));
            Assert.That(exitCount, Is.EqualTo(2));
            Assert.That(guardCount, Is.EqualTo(2));
            Assert.That(handle.CurrentState, Is.EqualTo("StateB"));
            Assert.That(handle.IsDestroyed, Is.False);
        }


        [Test]
        public void ManualHandleUpdate_HonorsInvalidationDuringStateUpdate()
        {
            var updateCount = 0;
            var guardCount = 0;
            var suspendOnce = true;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, 0, _testProcessingGroup)
                .State("StateA", null, context =>
                {
                    updateCount++;
                    if (suspendOnce)
                    {
                        suspendOnce = false;
                        context.IsValid = false;
                    }
                }, null)
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", _ => { guardCount++; return true; })
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext();
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            handle.Update(_testProcessingGroup);

            Assert.That(updateCount, Is.EqualTo(1));
            Assert.That(guardCount, Is.Zero);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(handle.IsDestroyed, Is.False);

            context.IsValid = true;
            handle.Update(_testProcessingGroup);

            Assert.That(updateCount, Is.EqualTo(2));
            Assert.That(guardCount, Is.EqualTo(1));
            Assert.That(handle.CurrentState, Is.EqualTo("StateB"));
            Assert.That(handle.IsDestroyed, Is.False);
        }


        [Test]
        public void FsmApiNamespaceFacade_CreatesAndUpdatesStringBackedMachine()
        {
            var enterCount = 0;
            var updateCount = 0;
            const string name = "FacadeFSM";

            global::FSM_API.FsmApi.Create.CreateFiniteStateMachine(name, -1, _testProcessingGroup)
                .State("Ready", _ => enterCount++, _ => updateCount++, null)
                .WithInitialState("Ready")
                .BuildDefinition();

            var context = new TestContext();
            var handle = global::FSM_API.FsmApi.Create.CreateInstance(
                name, context, _testProcessingGroup);

            global::FSM_API.FsmApi.Interact.Update(_testProcessingGroup);

            Assert.That(handle.CurrentState, Is.EqualTo("Ready"));
            Assert.That(enterCount, Is.EqualTo(1));
            Assert.That(updateCount, Is.EqualTo(1));
            Assert.That(handle.IsDestroyed, Is.False);
        }

        [Test]
        public void FsmApiNamespaceFacade_ForwardsErrorNotifications()
        {
            string observedMessage = null;
            global::TheSingularityWorkshop.FSM_API.FSM_API.Error.InternalApiErrorEventHandler handler =
                (message, exception) => observedMessage = message;

            global::FSM_API.FsmApi.Error.OnInternalApiError += handler;
            try
            {
                global::FSM_API.FsmApi.Error.InvokeInternalApiError("facade-test", null);
            }
            finally
            {
                global::FSM_API.FsmApi.Error.OnInternalApiError -= handler;
            }

            Assert.That(observedMessage, Is.EqualTo("facade-test"));
        }


        [Test]
        public void ManualEvaluateConditions_StopsWhenGuardInvalidatesContext()
        {
            var guardCount = 0;
            var exitCount = 0;
            var suspendOnce = true;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, 0, _testProcessingGroup)
                .State("StateA", null, null, _ => exitCount++)
                .State("StateB", null, null, null)
                .Transition("StateA", "StateB", context =>
                {
                    guardCount++;
                    if (suspendOnce)
                    {
                        suspendOnce = false;
                        context.IsValid = false;
                        return true;
                    }
                    return false;
                })
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext();
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            handle.EvaluateConditions();

            Assert.That(context.IsValid, Is.False);
            Assert.That(guardCount, Is.EqualTo(1));
            Assert.That(exitCount, Is.Zero);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(handle.IsDestroyed, Is.False);

            context.IsValid = true;
            handle.EvaluateConditions();

            Assert.That(guardCount, Is.EqualTo(2));
            Assert.That(exitCount, Is.Zero);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        }

        [Test]
        public void ForcedTransition_StopsAfterOnExitInvalidatesContext()
        {
            var targetEnterCount = 0;
            var suspendOnce = true;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, 0, _testProcessingGroup)
                .State("StateA", null, null, context =>
                {
                    if (suspendOnce)
                    {
                        suspendOnce = false;
                        context.IsValid = false;
                    }
                })
                .State("StateB", _ => targetEnterCount++, null, null)
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext();
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            handle.TransitionTo("StateB");

            Assert.That(context.IsValid, Is.False);
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
            Assert.That(targetEnterCount, Is.Zero);
            Assert.That(handle.IsDestroyed, Is.False);

            context.IsValid = true;
            handle.Update();

            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
            Assert.That(targetEnterCount, Is.Zero);
        }


        [Test]
        public void ForcedTransition_TargetEnterMaySuspendAfterCommittingTarget()
        {
            var targetEnterCount = 0;
            var targetUpdateCount = 0;

            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, 0, _testProcessingGroup)
                .State("StateA", null, null, null)
                .State("StateB", context =>
                {
                    targetEnterCount++;
                    context.IsValid = false;
                }, _ => targetUpdateCount++, null)
                .WithInitialState("StateA")
                .BuildDefinition();

            var context = new TestContext();
            var handle = FSM_API.Create.CreateInstance(_testFsmName, context, _testProcessingGroup);

            handle.TransitionTo("StateB");

            Assert.That(context.IsValid, Is.False);
            Assert.That(handle.CurrentState, Is.EqualTo("StateB"));
            Assert.That(handle.HasEnteredCurrentState, Is.True);
            Assert.That(targetEnterCount, Is.EqualTo(1));

            context.IsValid = true;
            handle.Update();

            Assert.That(targetEnterCount, Is.EqualTo(1));
            Assert.That(targetUpdateCount, Is.EqualTo(1));
            Assert.That(handle.CurrentState, Is.EqualTo("StateB"));
        }

    }
}
