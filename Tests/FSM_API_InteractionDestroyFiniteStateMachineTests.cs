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
    public class FSM_API_InteractionDestoryFiniteStateMachineTests
    {
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI();
        }

        /// <summary>
        /// 
        /// </summary>
        [TestFixture]
        public class FSM_API_InteractionDestructionTests
        {
            private const string _testProcessingGroup = "TestGroup";
            private const string _testFsmName = "TestFSM";

            /// <summary>
            /// 
            /// </summary>
            [SetUp]
            public void Setup()
            {
                // Reset the API before each test to ensure a clean state
                FSM_API.Internal.ResetAPI(true);
            }

            /// <summary>
            /// 
            /// </summary>
            [Test]
            public void DestroyFiniteStateMachine_ShouldCleanupAllInstances()
            {
                // Arrange
                // 1. Create a new FSM definition with some simple states
                FSM_API.Create.CreateFiniteStateMachine(_testFsmName, -1, _testProcessingGroup)
                    .State("StateA", TestStateActions.OnEnterStateA, null, null)
                    .WithInitialState("StateA")
                    .BuildDefinition();

                // 2. Create multiple instances of this FSM
                var ctx1 = new TestContext();
                var handle1 = FSM_API.Create.CreateInstance(_testFsmName, ctx1, _testProcessingGroup);

                var ctx2 = new TestContext();
                var handle2 = FSM_API.Create.CreateInstance(_testFsmName, ctx2, _testProcessingGroup);

                // 3. Verify the FSM and its instances exist before destruction
                Assert.That(FSM_API.Interaction.Exists(_testFsmName, _testProcessingGroup), Is.True, "FSM definition should exist before destruction.");
                Assert.That(FSM_API.Interaction.GetInstance(_testFsmName, ctx1, _testProcessingGroup), Is.Not.Null, "Instance 1 handle should be retrievable before destruction.");
                Assert.That(FSM_API.Interaction.GetInstance(_testFsmName, ctx2, _testProcessingGroup), Is.Not.Null, "Instance 2 handle should be retrievable before destruction.");

                // Act
                // Destroy the FSM definition and all its associated instances
                FSM_API.Interaction.DestroyFiniteStateMachine(_testFsmName, _testProcessingGroup);

                // Assert
                // 1. The FSM definition should no longer exist
                Assert.That(FSM_API.Interaction.Exists(_testFsmName, _testProcessingGroup), Is.False, "FSM definition should not exist after destruction.");

                // 2. All instances associated with the FSM should be gone.
                // Attempting to get their handles should now return null.
                Assert.That(FSM_API.Interaction.GetInstance(_testFsmName, ctx1, _testProcessingGroup), Is.Null, "Instance 1 handle should be null after destruction.");
                Assert.That(FSM_API.Interaction.GetInstance(_testFsmName, ctx2, _testProcessingGroup), Is.Null, "Instance 2 handle should be null after destruction.");

                // 3. The new assertion: Attempting to create a new instance of the now-destroyed
                // definition should throw an ArgumentException.
                Assert.Throws<KeyNotFoundException>(() =>
                {
                    // This is the action that is expected to throw the exception.
                    // An instance cannot be created from a non-existent definition.
                    FSM_API.Create.CreateInstance(_testFsmName, new TestContext(), _testProcessingGroup);
                });
            }
        }
    }
}
