using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Lifecycle_Tests
    {

        private List<string> _executionLog;
        private FSM _fsm;
        private MockContext _ctx;


        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _executionLog = new List<string>();
            _fsm = new FSM { Name = "LifecycleTestFSM" };
            _ctx = new MockContext();

            // Ensure clean API state
            FSM_API.Internal.ResetAPI(true);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Verify_Correct_State_Methods_Are_Called_During_Transition()
        {
            // 1. Arrange: Setup two states with distinct logging signatures
            FSM_API.Create.CreateFiniteStateMachine("LifeCycle", -1, "Update")
                .State("StateA",
                (ctx) => _executionLog.Add("Enter_A"),
                (ctx) => _executionLog.Add("Update_A"),
                (ctx) => _executionLog.Add("Exit_A"))
                .State("StateB",
                (ctx) => _executionLog.Add("Enter_B"),
                (ctx) => _executionLog.Add("Update_B"),
                (ctx) => _executionLog.Add("Exit_B"))


            // Add a transition from A -> B that triggers immediately
            .Transition("StateA", "StateB", (ctx) => true)
            .BuildDefinition();

            FSM_API.Create.CreateInstance("LifeCycle", new MockContext(), "Update");
            FSM_API.Interaction.Update();

            FSM_API.Interaction.Update();

            // 3. Assert: Verify the exact order to catch the "Initial State" glitch
            var expectedLog = new List<string>
            {
                "Enter_A",  // From EnterInitial
                "Update_A", // From Step 1
                "Exit_A",   // From Step 1 (Transition Fired)
                "Enter_B",  // From Manual Entry of Next State
                "Update_B"
            };

            Assert.That(_executionLog, Is.EqualTo(expectedLog),
                "The execution order was incorrect. If 'Enter_A' appears twice, the glitch is present.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Verify_Glitch_Regression_InitialState_Logic_Not_Called_On_Second_State()
        {
            // This test explicitly targets the specific bug you mentioned:
            // "Accidentally assign the initial state's method rather than the current state's"

            // Arrange
            bool initialStateEnterCalled = false;
            bool secondStateEnterCalled = false;

            _fsm.AddState(new FSMState("InitialState",
                (ctx) => initialStateEnterCalled = true,
                (ctx) => { },
                (ctx) => { }));

            _fsm.AddState(new FSMState("SecondState",
                (ctx) => secondStateEnterCalled = true,
                (ctx) => { },
                (ctx) => { }));

            _fsm.InitialState = "InitialState";
            _fsm.EnterInitial(_ctx); // Should set initialStateEnterCalled = true

            // Reset flag to ensure it doesn't get called AGAIN when we enter SecondState
            initialStateEnterCalled = false;

            // Act: Manually force entry into Second State
            var secondState = _fsm.GetState("SecondState");
            secondState.Enter(_ctx);

            // Assert
            Assert.That(secondStateEnterCalled, Is.True, "The SecondState OnEnter should have executed.");
            Assert.That(initialStateEnterCalled, Is.False,
                "CRITICAL FAILURE: The InitialState OnEnter was executed when entering SecondState.");
        }

       

        // Simple Mock Context for valid arguments
        private class MockContext : IStateContext
        {
            // Implement any required interface members here if IStateContext has them
            public bool IsValid { get; set; } = true;
            public string Name { get; set; }
        }


    }
}
