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
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMModifier_AddState_Succeeds()
        {
            // Arrange
            
            // Act
            FSM_API.Interaction.AddStateToFSM(FsmName, "AddedState", null, null, null, "Update");
           
           //Assert
            var fsmDef = FSM_API.Internal.GetFsmDefinition(FsmName, "Update");
            Assert.That(fsmDef, Is.Not.Null, "FSM definition should not be null");
            Assert.That(fsmDef.GetAllStates().ToList().Any(s=>s.Name == "AddedState"), Is.True, "FSM should contain the newly added state");
        }


        /// <summary>
        /// Tests that a state can be removed from an FSM using the Interaction API.
        /// </summary>
        [Test]
        public void FSMModifier_RemoveStateFromFSM_Succeeds()
        {
            // Arrange
            FSM_API.Interaction.AddStateToFSM(FsmName, "StateToRemove", null, null, null, ProcessGroup);
            var fsmDef = FSM_API.Internal.GetFsmDefinition(FsmName, ProcessGroup);
            var initialCount = fsmDef.GetAllStates().Count;

            // Act
            FSM_API.Interaction.RemoveStateFromFSM(FsmName, "StateToRemove", ProcessGroup);

            // Assert
            fsmDef = FSM_API.Internal.GetFsmDefinition(FsmName, ProcessGroup);
            Assert.That(fsmDef.GetAllStates().Count, Is.EqualTo(initialCount - 1), "State count should be one less after removal.");
            Assert.That(fsmDef.HasState("StateToRemove"), Is.False, "The state should no longer exist.");
        }

        /// <summary>
        /// Tests that a regular transition can be added between two states using the Interaction API.
        /// </summary>
        [Test]
        public void FSMModifier_AddTransition_Succeeds()
        {
            // Arrange
            FSM_API.Interaction.AddStateToFSM(FsmName, "StateA", null, null, null, ProcessGroup);
            FSM_API.Interaction.AddStateToFSM(FsmName, "StateB", null, null, null, ProcessGroup);
            var fsmDef = FSM_API.Internal.GetFsmDefinition(FsmName, ProcessGroup);
            var initialTransitionCount = fsmDef.GetAllTransitions().Count;

            // Act
            FSM_API.Interaction.AddTransition(FsmName, "StateA", "StateB", (ctx) => true, ProcessGroup);

            // Assert
            fsmDef = FSM_API.Internal.GetFsmDefinition(FsmName, ProcessGroup);
            Assert.That(fsmDef.GetAllTransitions().Count, Is.EqualTo(initialTransitionCount + 1), "A new transition should have been added.");
            Assert.That(fsmDef.HasTransition("StateA", "StateB"), Is.True, "The transition from StateA to StateB should exist.");
        }

        /// <summary>
        /// Tests that a regular transition can be removed using the Interaction API.
        /// </summary>
        [Test]
        public void FSMModifier_RemoveTransition_Succeeds()
        {
            // Arrange
            FSM_API.Interaction.AddStateToFSM(FsmName, "StateA", null, null, null, ProcessGroup);
            FSM_API.Interaction.AddStateToFSM(FsmName, "StateB", null, null, null, ProcessGroup);
            FSM_API.Interaction.AddTransition(FsmName, "StateA", "StateB", (ctx) => true, ProcessGroup);
            var fsmDef = FSM_API.Internal.GetFsmDefinition(FsmName, ProcessGroup);
            var initialTransitionCount = fsmDef.GetAllTransitions().Count;

            // Act
            FSM_API.Interaction.RemoveTransition(FsmName, "StateA", "StateB", ProcessGroup);

            // Assert
            fsmDef = FSM_API.Internal.GetFsmDefinition(FsmName, ProcessGroup);
            Assert.That(fsmDef.GetAllTransitions().Count, Is.EqualTo(initialTransitionCount - 1), "One transition should have been removed.");
            Assert.That(fsmDef.HasTransition("StateA", "StateB"), Is.False, "The transition from StateA to StateB should no longer exist.");
        }










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
