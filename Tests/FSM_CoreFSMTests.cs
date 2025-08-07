using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework; // Assuming NUnit for testing

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public class FSM_CoreFSMTests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true); // Resets the API for a clean test environment
        }

        [Test]
        public void AddState_Succeeds()
        {
            FSM fsm = new FSM();

            fsm.AddState(new FSMState("TestState",
                (ctx) => Console.WriteLine($"Entering TestState"),
                (ctx) => Console.WriteLine($"Updating TestState"),
                (ctx) => Console.WriteLine($"Exiting TestState")));

            Assert.IsTrue(fsm.GetAllStates().Count() == 1);
        }

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
            Assert.AreEqual(1, fsm.GetAllStates().Count, "Should have 1 state after adding the first version.");
            fsm.InitialState = "MyState"; // Set initial state for testing entry behavior

            // Define the second, updated version of "MyState"
            bool updatedActionCalled = false;
            fsm.AddState(new FSMState("MyState",
                (ctx) => updatedActionCalled = true, // Set flag when entered
                (ctx) => { },
                (ctx) => { }
            ));
            Assert.AreEqual(1, fsm.GetAllStates().Count, "Should still have 1 state after updating the existing state.");

            // Verify that the updated state is the one that gets used
            var ctx = new MockStateContext("MyState", fsm.Name);
            fsm.EnterInitial(ctx); // This will call the Enter action of "MyState"

            Assert.IsFalse(originalActionCalled, "Original state's Enter action should NOT have been called.");
            Assert.IsTrue(updatedActionCalled, "Updated state's Enter action should have been called.");
        }

        [Test]
        public void AddState_AddsMultipleDistinctStatesSucceeds()
        {
            FSM fsm = new FSM();

            fsm.AddState(new FSMState("StateA", null, null, null));
            fsm.AddState(new FSMState("StateB", null, null, null));
            fsm.AddState(new FSMState("StateC", null, null, null));

            Assert.AreEqual(3, fsm.GetAllStates().Count(), "Should have 3 states after adding distinct states.");
            Assert.IsTrue(fsm.HasState("StateA"));
            Assert.IsTrue(fsm.HasState("StateB"));
            Assert.IsTrue(fsm.HasState("StateC"));
            Assert.IsNotNull(fsm.GetState("StateA"));
            Assert.IsNotNull(fsm.GetState("StateB"));
            Assert.IsNotNull(fsm.GetState("StateC"));
        }



        [Test]
        public void AddTransition_Succeeds()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");
            fsm.AddTransition("StateA", "StateB", (ctx) => true);

            Assert.AreEqual(1, fsm.GetAllTransitions().Count, "Should have 1 transition after adding a valid one.");
            Assert.IsTrue(fsm.HasTransition("StateA", "StateB"), "Should confirm the transition exists.");
            Assert.IsNotNull(fsm.GetTransition(new Tuple<string, string>("StateB", "StateA")), "Should be able to retrieve the transition by (To, From).");
        }

        [Test]
        public void AddTransition_NullConditionThrowsArgumentNullException()
        {
            FSM fsm = new FSM { Name = "NullConditionTransitionFSM" };
            HelperAddState(fsm, "StateA");
            HelperAddState(fsm, "StateB");

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                fsm.AddTransition("StateA", "StateB", null);
            });

            Assert.That(ex.ParamName, Is.EqualTo("cond"), "The parameter name should be 'cond'.");
            Assert.That(ex.Message, Does.Contain($"Attempted to add a transition with null condition from 'StateA' to 'StateB' in FSM 'NullConditionTransitionFSM'."), "Error message should indicate null condition.");
            Assert.AreEqual(0, fsm.GetAllTransitions().Count, "No transition should be added when a null condition is provided.");
        }

       

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

            Assert.AreEqual(3, fsm.GetAllTransitions().Count(), "Should have 3 transitions after adding distinct ones.");
            Assert.IsTrue(fsm.HasTransition("StateA", "StateB"));
            Assert.IsTrue(fsm.HasTransition("StateB", "StateC"));
            Assert.IsTrue(fsm.HasTransition("StateA", "StateC"));
        }

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

            Assert.AreEqual(1, fsm.GetAllTransitions().Count, "Transition should be added to the blueprint even if 'from' state doesn't exist.");
            Assert.IsTrue(fsm.HasTransition("NonExistentState", "ExistingState"), "Should register the transition.");

            // To fully test this, you'd need a Step test where the FSM is current in "NonExistentState"
            // and observe error logging via FSM_API.Error.
        }

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

            Assert.AreEqual(1, fsm.GetAllTransitions().Count, "Transition should be added to the blueprint even if 'to' state doesn't exist.");
            Assert.IsTrue(fsm.HasTransition("ExistingState", "NonExistentState"), "Should register the transition.");

            // To fully test this, you'd need a Step test where the transition condition becomes true
            // and observe error logging via FSM_API.Error when trying to enter "NonExistentState".
        }

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

            Assert.AreEqual(3, fsm.GetAllTransitions().Count, "Initial count should be 3.");

            fsm.RemoveTransition("StateA", "StateB");

            Assert.AreEqual(2, fsm.GetAllTransitions().Count, "Count should be 2 after removing one transition.");
            Assert.IsFalse(fsm.HasTransition("StateA", "StateB"), "The specific transition A->B should be removed.");
            Assert.IsTrue(fsm.HasTransition("StateA", "StateC"), "Transition A->C should still exist.");
            Assert.IsTrue(fsm.HasTransition("StateB", "StateC"), "Transition B->C should still exist.");
        }


        [Test]
        public void RemoveState_Succeeds()
        {
            FSM fsm = new FSM();
            HelperAddState(fsm);

            fsm.RemoveState("TestState");

            Assert.IsTrue(fsm.GetAllStates().Count == 0);
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
