using NUnit.Framework; // Assuming NUnit for testing

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

using TheSingularityWorkshop.FSM_API.Tests.Internal;

namespace TheSingularityWorkshop.FSM_API.Tests
{

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_FSM_Tests
    {
        private List<(string message, Exception exception)> _capturedErrors;

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true); // Resets the API for a clean test environment
            _capturedErrors = new List<(string message, Exception exception)>();
            FSM_API.Error.OnInternalApiError += CaptureInternalApiError;
        }

        /// <summary>
        /// 
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            FSM_API.Error.OnInternalApiError -= CaptureInternalApiError; // Unsubscribe
                                                                         // Any other cleanup for FSM_API if needed
        }

        private void CaptureInternalApiError(string message, Exception exception)
        {
            _capturedErrors.Add((message, exception));
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddState_Succeeds()
        {
            FSM fsm = new FSM();

            fsm.AddState(new FSMState("TestState",
                (ctx) => Console.WriteLine($"Entering TestState"),
                (ctx) => Console.WriteLine($"Updating TestState"),
                (ctx) => Console.WriteLine($"Exiting TestState")));

            Assert.That(fsm.GetAllStates().Count(), Is.EqualTo(1));
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddState_WithInvalidNameThrows()
        {
            FSM fsm = new FSM();

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                fsm.AddState(new FSMState(null, null, null, null));
            });

            Assert.That(ex.Message, Does.Contain("null or empty"));
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddState_UpdateExistingStateSucceeds()
        {
            FSM fsm = new FSM { Name = "UpdateTestFSM", InitialState = "Initial" };

            // Define the first version of "MyState"
            bool originalActionCalled = false;
            fsm.AddState(new FSMState("MyState",
                (ctx) => originalActionCalled = true, // Set flag when entered
                (ctx) => { },
                (ctx) => { }
            ));
            Assert.That(fsm.GetAllStates().Count, Is.EqualTo(1), "Should have 1 state after adding the first version.");
            fsm.InitialState = "MyState"; // Set initial state for testing entry behavior

            // Define the second, updated version of "MyState"
            bool updatedActionCalled = false;
            fsm.AddState(new FSMState("MyState",
                (ctx) => updatedActionCalled = true, // Set flag when entered
                (ctx) => { },
                (ctx) => { }
            ));
            Assert.That(fsm.GetAllStates().Count, Is.EqualTo(1), "Should still have 1 state after updating the existing state.");

            // Verify that the updated state is the one that gets used
            var ctx = new MockStateContext("MyState", fsm.Name);
            fsm.EnterInitial(ctx); // This will call the Enter action of "MyState"

            Assert.That(originalActionCalled, Is.False, "Original state's Enter action should NOT have been called.");
            Assert.That(updatedActionCalled, Is.True, "Updated state's Enter action should have been called.");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddState_AddsMultipleDistinctStatesSucceeds()
        {
            FSM fsm = new FSM();

            fsm.AddState(new FSMState("StateA", null, null, null));
            fsm.AddState(new FSMState("StateB", null, null, null));
            fsm.AddState(new FSMState("StateC", null, null, null));

            Assert.That(fsm.GetAllStates().Count(), Is.EqualTo(3), "Should have 3 states after adding distinct states.");
            Assert.That(fsm.HasState("StateA"), Is.True);
            Assert.That(fsm.HasState("StateB"), Is.True);
            Assert.That(fsm.HasState("StateC"), Is.True);
            Assert.That(fsm.GetState("StateA"), Is.Not.Null);
            Assert.That(fsm.GetState("StateB"), Is.Not.Null);
            Assert.That(fsm.GetState("StateC"), Is.Not.Null);
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddTransition_Succeeds()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            fsm.AddTransition("StateA", "StateB", (ctx) => true);

            Assert.That(fsm.GetAllTransitions().Count, Is.EqualTo(1), "Should have 1 transition after adding a valid one.");
            Assert.That(fsm.HasTransition("StateA", "StateB"), Is.True, "Should confirm the transition exists.");
            Assert.That(fsm.GetTransition(new Tuple<string, string>("StateB", "StateA")), Is.Not.Null, "Should be able to retrieve the transition by (To, From).");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddTransition_NullConditionInvokesInternalApiError()
        {
            // Arrange
            var fsm = new FSM();
            fsm.Name = "TestFSM";
            fsm.AddState(new FSMState("StateA", null, null, null));
            fsm.AddState(new FSMState("StateB", null, null, null));

            // Act
            fsm.AddTransition("StateA", "StateB", null); // Pass null condition

            // Assert
            Assert.That(_capturedErrors, Is.Not.Empty, "OnInternalApiError event should have been invoked.");
            Assert.That(_capturedErrors[0].exception, Is.InstanceOf<ArgumentNullException>(), "Captured exception should be ArgumentNullException.");
            Assert.That(_capturedErrors[0].message, Does.Contain("Attempted to add a transition with null condition"), "Captured message should indicate null condition.");
            //Assert.That(_capturedErrors[0].exception., Is.EqualTo("cond"), "ArgumentNullException should be for 'cond' parameter.");

            // Also assert that the transition was *not* added
            Assert.That(fsm.GetTransition(new Tuple<string, string>("StateB", "StateA")), Is.Null, "Transition should not be added when condition is null.");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddAnyStateTransition_NullConditionInvokesInternalApiError()
        {
            // Arrange
            var fsm = new FSM();
            fsm.Name = "TestFSM";
            fsm.AddState(new FSMState("StateA", null, null, null));

            // Act
            fsm.AddAnyStateTransition("StateA", null);

            // Assert
            Assert.That(_capturedErrors, Is.Not.Empty, "OnInternalApiError event should have been invoked for AnyState transition.");
            Assert.That(_capturedErrors[0].exception, Is.InstanceOf<ArgumentNullException>(), "Captured exception for AnyState should be ArgumentNullException.");
            Assert.That(_capturedErrors[0].message, Does.Contain("Attempted to add an Any-State transition with null condition"), "Captured message should indicate null condition for AnyState.");
            //Assert.That(_capturedErrors[0].exception.ParamName, Is.EqualTo("cond"), "ArgumentNullException should be for 'cond' parameter.");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddTransition_AddsMultipleDistinctTransitionsSucceeds()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            HelperAddState(fsm, "StateC");

            fsm.AddTransition("StateA", "StateB", (ctx) => true);
            fsm.AddTransition("StateB", "StateC", (ctx) => true);
            fsm.AddTransition("StateA", "StateC", (ctx) => true);

            Assert.That(fsm.GetAllTransitions().Count(), Is.EqualTo(3), "Should have 3 transitions after adding distinct ones.");
            Assert.That(fsm.HasTransition("StateA", "StateB"), Is.True);
            Assert.That(fsm.HasTransition("StateB", "StateC"), Is.True);
            Assert.That(fsm.HasTransition("StateA", "StateC"), Is.True);
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddTransition_FromNonExistentStateNoThrows()
        {
            FSM fsm = new FSM { Name = "FromNonExistentStateFSM" };
            HelperAddState(fsm, "ExistingState");

            // Adding a transition FROM a non-existent state should NOT throw at AddTransition time.
            // The FSM definition is merely a blueprint; validation happens during Step.
            Assert.DoesNotThrow(() =>
            {
                fsm.AddTransition("NonExistentState", "ExistingState", (ctx) => true);
            });

            Assert.That(fsm.GetAllTransitions().Count, Is.EqualTo(1), "Transition should be added to the blueprint even if 'from' state doesn't exist.");
            Assert.That(fsm.HasTransition("NonExistentState", "ExistingState"), Is.True, "Should register the transition.");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddTransition_ToNonExistentStateNoThrows()
        {
            FSM fsm = new FSM { Name = "ToNonExistentStateFSM" };
            HelperAddState(fsm, "ExistingState");

            // Adding a transition TO a non-existent state should NOT throw at AddTransition time.
            Assert.DoesNotThrow(() =>
            {
                fsm.AddTransition("ExistingState", "NonExistentState", (ctx) => true);
            });

            Assert.That(fsm.GetAllTransitions().Count, Is.EqualTo(1), "Transition should be added to the blueprint even if 'to' state doesn't exist.");
            Assert.That(fsm.HasTransition("ExistingState", "NonExistentState"), Is.True, "Should register the transition.");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void RemoveTransition_SpecificTransitionRemoved()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            HelperAddState(fsm, "StateC");

            fsm.AddTransition("StateA", "StateB", (ctx) => true);
            fsm.AddTransition("StateA", "StateC", (ctx) => true);
            fsm.AddTransition("StateB", "StateC", (ctx) => true);

            Assert.That(fsm.GetAllTransitions().Count, Is.EqualTo(3), "Initial count should be 3.");

            fsm.RemoveTransition("StateA", "StateB");

            Assert.That(fsm.GetAllTransitions().Count, Is.EqualTo(2), "Count should be 2 after removing one transition.");
            Assert.That(fsm.HasTransition("StateA", "StateB"), Is.False, "The specific transition A->B should be removed.");
            Assert.That(fsm.HasTransition("StateA", "StateC"), Is.True, "Transition A->C should still exist.");
            Assert.That(fsm.HasTransition("StateB", "StateC"), Is.True, "Transition B->C should still exist.");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void RemoveState_Succeeds()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm);

            fsm.RemoveState("TestState");

            Assert.That(fsm.GetAllStates().Count, Is.EqualTo(0));
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void RemoveState_WithManyStatesSucceeds()
        {
            FSM fSM = new FSM();
            for (int i = 0; i < 5; i++)
            {
                HelperAddState(fSM, $"TestState{i}");
            }

            var initCount = fSM.GetAllStates().Count;
            fSM.RemoveState("TestState2");
            Assert.That(fSM.GetAllStates().Count, Is.EqualTo(initCount - 1), "Should have one less state after removal.");
            Assert.That(fSM.HasState("TestState2"), Is.False, "Should confirm the state2 was removed.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ForceTransition_Succeeds()
        {
            FSM fsm = new FSM();
           
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HasTransition_ReturnsFalseWhenEmpty()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            Assert.That(fsm.HasTransition("StateA", "StateB"), Is.False, "Should return false when no transitions exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HasTransition_ReturnsFalseForNonExistentTransition()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            fsm.AddTransition("StateA", "StateB", (ctx) => true);
            Assert.That(fsm.HasTransition("StateB", "StateA"), Is.False, "Should return false for reverse transition that doesn't exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HasState_ReturnsFalseWhenEmpty()
        {
            FSM fsm = new FSM();
            Assert.That(fsm.HasState("NonExistentState"), Is.False, "Should return false when no states exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HasState_ReturnsFalseForNonExistentState()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "ExistingState");
            Assert.That(fsm.HasState("NonExistentState"), Is.False, "Should return false for a state that doesn't exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetState_ReturnsNullForNonExistentState()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "ExistingState");
            var state = fsm.GetState("NonExistentState");
            Assert.That(state, Is.Null, "Should return null for a state that doesn't exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetState_ReturnsCorrectState()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "TestState");
            var state = fsm.GetState("TestState");
            Assert.That(state, Is.Not.Null, "Should return the state that exists.");
            Assert.That(state.Name, Is.EqualTo("TestState"), "Should return the correct state by name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllStates_ReturnsEmptyWhenNoStates()
        {
            FSM fsm = new FSM();
            var states = fsm.GetAllStates();
            Assert.That(states, Is.Empty, "Should return an empty collection when no states exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllStates_ReturnsAllAddedStates()
        {
            FSM fsm = new FSM();
            for (int i = 0; i < 3; i++)
            {
                HelperAddState(fsm, $"TestState{i}");
            }
            var states = fsm.GetAllStates();
            Assert.That(states.Count(), Is.EqualTo(3), "Should return all added states.");
            Assert.That(states.Select(s => s.Name).OrderBy(n => n), Is.EqualTo(new[] { "TestState0", "TestState1", "TestState2" }));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllTransitions_ReturnsEmptyWhenNoTransitions()
        {
            FSM fsm = new FSM();
            var transitions = fsm.GetAllTransitions();
            Assert.That(transitions, Is.Empty, "Should return an empty collection when no transitions exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllTransitions_ReturnsAllAddedTransitions()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            HelperAddState(fsm, "StateC");
            fsm.AddTransition("StateA", "StateB", (ctx) => true);
            fsm.AddTransition("StateB", "StateC", (ctx) => true);
            fsm.AddTransition("StateA", "StateC", (ctx) => true);
            var transitions = fsm.GetAllTransitions();
            Assert.That(transitions.Count(), Is.EqualTo(3), "Should return all added transitions.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetTransition_ReturnsNullForNonExistentTransition()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            var transition = fsm.GetTransition(new Tuple<string, string>("StateA", "StateB"));
            Assert.That(transition, Is.Null, "Should return null for a transition that doesn't exist.");
        }

       


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AddAnyStateTransition_Succeeds()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");

            // Add an Any-State transition
            fsm.AddAnyStateTransition("StateA", (ctx) => true);

            // Verify the transition was added
            var anyTransitions = fsm.GetAnyStateTransitions();
            Assert.That(anyTransitions.Count, Is.EqualTo(1));
            Assert.That(anyTransitions[0].To, Is.EqualTo("StateA"));
            Assert.That(anyTransitions[0].From, Is.EqualTo(FSM.AnyStateIdentifier));
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void RemoveState_AlsoRemovesAssociatedTransitions()
        {
            // Arrange
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            HelperAddState(fsm, "StateC");

            fsm.AddTransition("StateA", "StateB", (ctx) => true); // A -> B
            fsm.AddTransition("StateB", "StateA", (ctx) => true); // B -> A
            fsm.AddAnyStateTransition("StateC", (ctx) => true);   // Any -> C

            Assert.That(fsm.GetAllStates().Count, Is.EqualTo(3), "Initial state count should be 3.");
            Assert.That(fsm.GetAllTransitions().Count, Is.EqualTo(3), "Initial transition count should be 3.");

            // Act: Remove the middle state
            fsm.RemoveState("StateB");

            // Assert
            Assert.That(fsm.GetAllStates().Count, Is.EqualTo(2), "State count should be 2 after removing one state.");
            Assert.That(fsm.HasState("StateB"), Is.False, "State 'StateB' should be removed.");

            // Verify that transitions associated with "StateB" are also gone.
            Assert.That(fsm.HasTransition("StateA", "StateB"), Is.False, "Transition A->B should be removed.");
            Assert.That(fsm.HasTransition("StateB", "StateA"), Is.False, "Transition B->A should be removed.");
            Assert.That(fsm.HasTransition(FSM.AnyStateIdentifier, "StateC"), Is.True, "Any-State transition should still exist.");
        }


        /// <summary>
        /// Tests that adding an "Any-State" transition using the FSMTransition object overload succeeds.
        /// </summary>
        [Test]
        public void AddAnyStateTransition_UsingFSMTransitionObject_Succeeds()
        {
            // Arrange
            FSM fsm = new FSM { Name = "AnyStateOverloadTest" };
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");

            // Create an FSMTransition object
            var transition = new FSMTransition(FSM.AnyStateIdentifier, "StateB", (ctx) => true);

            // Act
            fsm.AddAnyStateTransition(transition);

            // Assert
            var anyTransitions = fsm.GetAnyStateTransitions();
            Assert.That(anyTransitions.Count, Is.EqualTo(1));
            Assert.That(anyTransitions.Any(t => t.To == "StateB"), Is.True, "Any-State transition to StateB should exist.");
        }

       


        private void HelperAddState(FSM fsm, string stateName = "TestState")
        {
            fsm.AddState(new FSMState(stateName,
               (ctx) => Console.WriteLine($"Entering {stateName}"),
               (ctx) => Console.WriteLine($"Updating {stateName}"),
               (ctx) => Console.WriteLine($"Exiting {stateName}")));
        }
    }
}
