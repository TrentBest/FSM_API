using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// A mock state context for testing purposes.
    /// In a real application, this would be an object with data specific to the FSM.
    /// </summary>
    public class MockStateContext : IStateContext
    {

        /// <summary>
        /// Mock Constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>

        public MockStateContext(string value = "TestValue", string name = "TestName")
        {
            Value = value;
            Name = name;
        }


        /// <summary>
        /// Mock Data
        /// </summary>
        public string ContextData { get; set; } = "Initial";

        /// <summary>
        /// Mock IsValid
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Mock Name
        /// </summary>
        public string Name { get; set; } = "MockState";

        /// <summary>
        /// Mock value
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// 
        /// </summary>
        public bool HasEnteredCurrentState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool CanTransitionToB { get; internal set; }

    }

    /// <summary>
    /// Unit tests for the FSMModifier class and its runtime behavior,
    /// using the NUnit framework.
    /// </summary>
    [TestFixture]
    public class FSM_API_FSMModifier_Tests
    {
        private const string FsmName = "TestFSM";
        private const string ProcessGroup = "Update";

        private static void FSM_Setup()
        {
            // Create a simple FSM to use for modification tests
            FSM_API.Create.CreateFiniteStateMachine(FsmName)
                .State("Idle",
                    onEnter: (ctx) => { ((MockStateContext)ctx).ContextData = "Idle"; },
                    onUpdate: null,
                    onExit: null)
                .State("Running",
                    onEnter: (ctx) => { ((MockStateContext)ctx).ContextData = "Running"; },
                    onUpdate: null,
                    onExit: null)
                .Transition("Idle", "Running", (ctx) => true)
                .Transition("Running", "Idle", (ctx) => ctx is MockStateContext mock && mock.ContextData == string.Empty)
                .BuildDefinition();
        }

        /// <summary>
        /// Setup for tests, resets the API and creates a predefined FSM for testing.
        /// </summary>

        [SetUp]
        public void Setup()
        {
            // Reset the API for a clean test environment before each test.
            FSM_API.Internal.ResetAPI(true);
            FSM_Setup();
        }

        ///// <summary>
        ///// Tests that a State and Transition can be added after definition.
        ///// </summary>
        //[Test]
        //public void Test_AddStateAndTransition_Runtime()
        //{
        //    // Arrange
        //    // Ensure the initial FSM definition has two states and one transition
        //    Assert.That(FSM_API.Internal.DoesFsmDefinitionExist(ProcessGroup, FsmName), Is.True);
        //    var initialDefinition = FSM_API.Internal.GetBucket(ProcessGroup, FsmName)?.Definition;
        //    Assert.That(initialDefinition, Is.Not.Null);
        //    Assert.That(initialDefinition.GetAllStates().Count, Is.EqualTo(2));

        //    // Create an instance of the FSM
        //    var context = new MockStateContext();
        //    var handle = FSM_API.Create.CreateInstance(FsmName, context);
        //    Assert.That(handle, Is.Not.Null);

        //    // Act
        //    // Use FSMModifier to add a new state "Paused" and a new transition
        //    FSM_API.Interaction.AddStateToFSM(FsmName, "Paused", null, null, null);
 

        //    // Get the modified definition
        //    var modifiedDefinition = FSM_API.Internal.GetBucket(ProcessGroup, FsmName)?.Definition;
        //    Assert.That(modifiedDefinition, Is.Not.Null);

        //    // Assert
        //    // Verify that the new state exists
        //    Assert.That(modifiedDefinition.GetAllStates().Count, Is.EqualTo(3));
        //    Assert.That(modifiedDefinition.GetState("Paused"), Is.Not.Null);

        //    // Verify that the new transition exists
        //    var newTransition = modifiedDefinition.GetTransition(new Tuple<string, string>("Running", "Pause"));
        //    Assert.That(newTransition, Is.Not.Null);
        //    Assert.That(newTransition.From, Is.EqualTo("Running"));
        //    Assert.That(newTransition.To, Is.EqualTo("Paused"));

        //    // Verify that an FSM instance can use the new transition
        //    Assert.That(context.ContextData, Is.EqualTo("Initial"));

        //    // Trigger the initial transition (Idle -> Running)
        //    handle.TransitionTo("Start");

        //    // Trigger the newly added transition (Running -> Paused)
        //    Assert.That(context.ContextData, Is.EqualTo("Running"));
        //    handle.TransitionTo("Pause");

        //    // Check that the FSM instance is now in the new "Paused" state
        //    // (Note: The FSM's state change is handled internally by the API,
        //    // we're simulating the state by checking the context data)
        //    Assert.That(context.ContextData, Is.EqualTo("Paused"));
        //}

        ///// <summary>
        ///// Tests removing of a state after definition.
        ///// </summary>
        //[Test]
        //public void Test_RemoveState_Runtime()
        //{
        //    // Arrange
        //    var initialDefinition = FSM_API.Internal.GetBucket(ProcessGroup, FsmName)?.Definition;
        //    Assert.That(initialDefinition!.GetAllStates().Count, Is.EqualTo(2));
        //    Assert.That(initialDefinition!.GetState("Running"), Is.Not.Null);

        //    // Act
        //    FSM_API.Interaction.RemoveStateFromFSM(FsmName, "Running", "Idle", ProcessGroup);

        //    // Get the modified definition
        //    var modifiedDefinition = FSM_API.Internal.GetBucket(ProcessGroup, FsmName)?.Definition;

        //    // Assert
        //    // The "Running" state should no longer exist
        //    Assert.That(modifiedDefinition!.GetAllStates().Count, Is.EqualTo(1));
        //    Assert.That(modifiedDefinition!.GetState("Running"), Is.Null);

        //    // The transition from "Idle" to "Running" should also be removed
        //    var removedTransition = modifiedDefinition.GetTransition(new Tuple<string, string>("Idle", "Start"));
        //    Assert.That(removedTransition, Is.Null);
        //}

        ///// <summary>
        ///// Tests modification of a Transition after definition.
        ///// </summary>
        //[Test]
        //public void Test_ModifyTransition_Runtime()
        //{
        //    // Arrange
        //    var initialDefinition = FSM_API.Internal.GetBucket(ProcessGroup, FsmName)?.Definition;
        //    var context = new MockStateContext();
        //    var handle = FSM_API.Create.CreateInstance(FsmName, context);

        //    // Act - Modify the transition condition
        //    FSM_API.Interaction.AddTransition(FsmName, "Idle", "Start", (ctx) => ((MockStateContext)ctx).ContextData == "NewCondition");
        

        //    // Try to trigger the transition with the old condition, should fail
        //    handle.TransitionTo("Start");
        //    Assert.That(context.ContextData, Is.EqualTo("Initial")); // Should still be "Initial"

        //    // Update the context to meet the new condition
        //    context.ContextData = "NewCondition";

        //    // Try to trigger the transition again, should now succeed
        //    handle.TransitionTo("Start");
        //    Assert.That(context.ContextData, Is.EqualTo("Running"));
        //}
    }
    /// <summary>
    /// 
    /// </summary>
    public class MockState : FSMState
    {
        /// <summary>
        /// 
        /// </summary>
        public bool ExitCalled { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public IStateContext ExitContext { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>

        public MockState(string name) : base(name, null, null, null) { }

    }
}
