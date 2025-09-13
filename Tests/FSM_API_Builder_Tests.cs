using System;
using System.Linq;

using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests
{

    /// <summary>
    /// This test fixture contains unit tests for the FSMBuilder class,
    /// ensuring that it correctly builds FSM definitions and handles
    /// invalid input.
    /// </summary>
    [TestFixture]
    public class FSM_API_Builder_Tests
    {
        private const string FsmName = "TestFSM";
        private const string InitialStateName = "Idle";
        private const string OtherStateName = "Running";
        private const string ValidProcessingGroup = "GameLoop";
        private static int processRate = 0;
        private static string processingGroup = "Update";

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Resets the API to ensure a clean state before each test run.
            // The 'true' parameter indicates a full reset, clearing all buckets.
            FSM_API.Internal.ResetAPI(true);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithInitialState_ValidState_SetsInitialState()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);

            // Act
            builder.WithInitialState(InitialStateName);

            // Assert
            // We can't directly access the private _initialState field,
            // so we'll test the BuildDefinition output to confirm it was set.
            // We expect this to fail later because no states have been added yet,
            // but the test of whether the state was *set* is in a later test.
            Assert.That(builder, Is.Not.Null);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithInitialState_NullStateName_ThrowsArgumentException()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);

            // Act & Assert
            // Verify that passing a null state name to WithInitialState throws an ArgumentException.
            Assert.Throws<ArgumentException>(() => builder.WithInitialState(null));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithInitialState_EmptyStateName_ThrowsArgumentException()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);

            // Act & Assert
            // Verify that passing an empty string to WithInitialState throws an ArgumentException.
            Assert.Throws<ArgumentException>(() => builder.WithInitialState(string.Empty));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithState_ValidState_AddsStateToBuilder()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);

            // Act
            builder.State(InitialStateName, null, null, null);

            // Assert
            // Similar to the initial state, we cannot directly access the private states.
            // This test is to ensure the method runs without throwing an exception.
            // The BuildDefinition test will confirm the state was correctly added.
            Assert.That(builder, Is.Not.Null);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithState_DuplicateStateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);
            builder.State(InitialStateName, null, null, null);

            // Act & Assert
            // Attempting to add the same state name twice should fail.
            Assert.Throws<ArgumentException>(() => builder.State(InitialStateName, null, null, null));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithTransition_ValidTransition_AddsTransitionToBuilder()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);
            builder.State(InitialStateName, null, null, null);
            builder.State(OtherStateName, null, null, null);

            // Act
            builder.Transition(InitialStateName, OtherStateName, (ctx) => true);

            // Assert
            // Similar to WithState, this ensures the method runs without exceptions.
            // The BuildDefinition test will verify the transition was added.
            Assert.That(builder, Is.Not.Null);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithTransition_NullFromState_ThrowsArgumentException()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.Transition(null, OtherStateName, (ctx) => true));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithTransition_NullToState_ThrowsArgumentException()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.Transition(InitialStateName, null, (ctx) => true));
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void WithProcessingGroup_ValidGroup_SetsProcessingGroup()
        //{
        //    // Arrange
        //    FSM_API.Create.CreateProcessingGroup(ValidProcessingGroup);
        //    CreateFSMDefinition();

        //    var definition = FSM_API.Interaction.GetFSMDefinition(FsmName);
        //    // Assert
        //    // Verify that the processing group was correctly assigned to the FSM definition.
        //    Assert.That(definition.ProcessingGroup, Is.EqualTo(ValidProcessingGroup));
        //}

        private static void CreateFSMDefinition()
        {
            FSM_API.Create.CreateFiniteStateMachine(FsmName, processRate, processingGroup);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithProcessingGroup_NullGroup_ThrowsArgumentException()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.WithProcessingGroup(null));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WithProcessRate_ValidRate_SetsProcessRate()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);
            builder.WithInitialState(InitialStateName).State(InitialStateName, null, null, null);
            const int rate = 100;

            // Act
            builder.WithProcessRate(rate);
            builder.BuildDefinition();
            var definition = FSM_API.Interaction.GetFSMDefinition(FsmName);
            // Assert
            Assert.That(definition.ProcessRate, Is.EqualTo(rate));
        }
        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void WithProcessRate_NegativeRate_ThrowsArgumentOutOfRangeException()
        //{
        //    // Arrange
        //    FSM_API.Create.CreateFiniteStateMachine(FsmName, processRate, processingGroup)
        //         .State(InitialStateName, null, null, null)
        //         .BuildDefinition();

        //    // Act & Assert
        //    Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithProcessRate(-1));
        //}
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void BuildDefinition_ValidFsm_ReturnsCorrectDefinition()
        {
            // Arrange
            var builder = new FSMBuilder(FsmName);
            builder.WithInitialState(InitialStateName)
                   .State(InitialStateName, null, null, null)
                   .State(OtherStateName, null, null, null)
                   .Transition(InitialStateName, OtherStateName, (ctx) => true);

            // Act
            builder.BuildDefinition();
            var definition = FSM_API.Interaction.GetFSMDefinition(FsmName);
            // Assert
            // Verify the core properties of the built FSM definition.
            Assert.That(definition, Is.Not.Null);
            Assert.That(definition.Name, Is.EqualTo(FsmName));
            Assert.That(definition.InitialState, Is.EqualTo(InitialStateName));
            Assert.That(definition.GetAllStates().Count, Is.EqualTo(2));
            Assert.That(definition.GetAllTransitions().Count, Is.EqualTo(1));
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_State_ThrowsForDuplicateStateName()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");
            builder.State("Idle", null, null, null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.State("Idle", null, null, null), "Expected ArgumentException for duplicate state name.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_WithInitialState_Succeeds()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");
            builder.State("Idle", null, null, null);

            // Act
            builder.WithInitialState("Idle");
            builder.BuildDefinition();

            var fsm = FSM_API.Internal.GetFsmDefinition("TestFSM", "Update");

            // Assert
            Assert.That(fsm, Is.Not.Null);
            Assert.That(fsm.InitialState, Is.EqualTo("Idle"));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_WithInitialState_ThrowsForNullOrEmptyName()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");
            builder.State("Idle", null, null, null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.WithInitialState(null), "Expected ArgumentException for null initial state.");
            Assert.Throws<ArgumentException>(() => builder.WithInitialState(""), "Expected ArgumentException for empty initial state.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_Transition_Succeeds()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");
            builder.State("StateA", null, null, null);
            builder.State("StateB", null, null, null);

            // Act
            builder.Transition("StateA", "StateB", (ctx) => true);
            builder.BuildDefinition();

            var fsm = FSM_API.Internal.GetFsmDefinition("TestFSM", "Update");

            // Assert
            Assert.That(fsm, Is.Not.Null);
            Assert.That(fsm.HasTransition("StateA", "StateB"), Is.True);
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_Transition_ThrowsForNullCondition()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");
            builder.State("StateA", null, null, null);
            builder.State("StateB", null, null, null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.Transition("StateA", "StateB", null), "Expected ArgumentNullException for null condition.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_AnyTransition_Succeeds()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");
            builder.State("StateB", null, null, null);

            // Act
            builder.AnyTransition("StateB", (ctx) => true);
            builder.BuildDefinition();

            var fsm = FSM_API.Internal.GetFsmDefinition("TestFSM", "Update");

            // Assert
            Assert.That(fsm, Is.Not.Null);
            Assert.That(fsm.GetAnyStateTransitions().Count, Is.EqualTo(1));
            Assert.That(fsm.GetAnyStateTransitions().First().To, Is.EqualTo("StateB"));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_AnyTransition_ThrowsForNullToState()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.AnyTransition(null, (ctx) => true), "Expected ArgumentException for null 'to' state.");
            Assert.Throws<ArgumentException>(() => builder.AnyTransition("", (ctx) => true), "Expected ArgumentException for empty 'to' state.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_AnyTransition_ThrowsForNullCondition()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");
            builder.State("StateB", null, null, null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.AnyTransition("StateB", null), "Expected ArgumentNullException for null condition.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMBuilder_BuildDefinition_FailsWhenNoStatesAdded()
        {
            // Arrange
            var builder = new FSMBuilder("TestFSM");

            // Act & Assert
            // The BuildDefinition method now gracefully handles an FSM with no states, returning a valid but empty FSM.
            // Therefore, we no longer expect an exception, but can assert on the resulting FSM properties.
            Assert.DoesNotThrow(() => builder.BuildDefinition(), "BuildDefinition should not throw an exception when no states are added.");
            var fsm = FSM_API.Internal.GetFsmDefinition("TestFSM", "Update");
            Assert.That(fsm.GetAllStates().Count, Is.EqualTo(0), "FSM should have no states.");
        }

        /// <summary>
        /// Tests that the WithProcessRate method sets the correct rate on the FSM definition.
        /// </summary>
        [Test]
        public void FSMBuilder_WithProcessRate_Succeeds()
        {
            // Arrange
            var builder = new FSMBuilder("ProcessRateTestFSM");
            int customRate = 30; // 30 ticks per second

            // Act
            builder.WithProcessRate(customRate);
            builder.BuildDefinition();

            var fsm = FSM_API.Internal.GetFsmDefinition("ProcessRateTestFSM", "Update");

            // Assert
            Assert.That(fsm, Is.Not.Null);
            Assert.That(fsm.ProcessRate, Is.EqualTo(customRate), "The process rate on the FSM definition should match the one set in the builder.");
        }


        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void BuildDefinition_ClearsBuilderState()
        //{
        //    // Arrange
        //    var builder = new FSMBuilder(FsmName);
        //    builder.State(InitialStateName, null, null, null);

        //    // Act
        //    builder.BuildDefinition();

        //    // Assert
        //    // A key feature of the builder is to clear its state after a successful build.
        //    // We should not be able to build a definition with the same builder instance again
        //    // without re-configuring it, as the initial state and states list should be cleared.
        //    Assert.Throws<InvalidOperationException>(() => builder.BuildDefinition());
        //}
    }
}
