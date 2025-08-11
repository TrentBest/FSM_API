using NUnit.Framework;

using System;

using TheSingularityWorkshop.FSM_API;


namespace TheSingularityWorkshop.FSM_API.Tests
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
    public class FSM_API_InteractionUpdateTests
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

        

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void Update_ShouldExecuteOnEnterAndOnUpdate_WhenTransitionOccurs()
        //{
        //    // Arrange
        //    FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
        //        .State("StateA", TestStateActions.OnEnterStateA, TestStateActions.OnUpdateStateA, TestStateActions.OnExitStateA)
        //        .State("StateB", null, null, null)
        //        .Transition("StateA", "StateB", TestStateActions.ShouldTransition)
        //        .WithInitialState("StateA")
        //        .BuildDefinition();

        //    var ctx = new TestContext();
        //    var handle = FSM_API.Create.CreateInstance(_testFsmName, ctx, _testProcessingGroup);
        //    //InitialState:
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx.OnEnterCounter, Is.EqualTo(0));
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(0));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(0));
        //    // Act 1: Initial state check (First update tick)
        //    // OnEnter should be called. OnUpdate should not.
        //    FSM_API.Interaction.Update(_testProcessingGroup);
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(0));

        //    // Act 2: Second update tick (OnUpdate should be called, transition should be evaluated, but will be false)
        //    FSM_API.Interaction.Update(_testProcessingGroup);
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        //    Assert.That(ctx.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(2));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(0));

        //    // Act 3: Trigger transition and update again (Third update tick)
        //    // This time, the transition condition will be true, leading to a state change.
        //    handle.TransitionTo("StateB");
        //    FSM_API.Interaction.Update(_testProcessingGroup);

        //    // Assert
        //    Assert.That(handle.CurrentState, Is.EqualTo("StateB"));
        //    // OnEnterStateB is null, so OnEnterCounter should not increase.
        //    // OnUpdate and OnExit from StateA should have been called once more, but the test's logic is that the update loop runs first.
        //    Assert.That(ctx.OnEnterCounter, Is.EqualTo(1));
        //    Assert.That(ctx.OnUpdateCounter, Is.EqualTo(3));
        //    Assert.That(ctx.OnExitCounter, Is.EqualTo(1));
        //}

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
    }
}
