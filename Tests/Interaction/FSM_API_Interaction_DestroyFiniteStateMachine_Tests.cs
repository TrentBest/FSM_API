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
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Interaction_DestoryFiniteStateMachine_Tests
    {
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI();
            FSM_API.Create.CreateFiniteStateMachine(_testFsmName, processingGroup: _testProcessingGroup).BuildDefinition();
        }


        private const string _testProcessingGroup = "TestGroup";
        private const string _testFsmName = "TestFSM";



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

        /// <summary>
        /// Tests that a registered FSM definition is successfully destroyed.
        /// </summary>
        [Test]
        public void DestroyFiniteStateMachine_Succeeds()
        {
            // Arrange
            Assert.That(FSM_API.Internal.DoesFsmDefinitionExist(_testProcessingGroup, _testFsmName), Is.True);

            // Act
            FSM_API.Interaction.DestroyFiniteStateMachine(_testFsmName, _testProcessingGroup);

            // Assert
            Assert.That(FSM_API.Internal.DoesFsmDefinitionExist(_testProcessingGroup, _testFsmName), Is.False);
        }

        /// <summary>
        /// Tests that attempting to destroy a non-existent FSM does not cause an exception.
        /// </summary>
        [Test]
        public void DestroyFiniteStateMachine_NonExistentFsm_DoesNotThrow()
        {
            // Arrange
            string nonExistentFsm = "NonExistentFSM";
            Assert.That(FSM_API.Internal.DoesFsmDefinitionExist(_testProcessingGroup, nonExistentFsm), Is.False);

            // Act & Assert
            Assert.DoesNotThrow(() => FSM_API.Interaction.DestroyFiniteStateMachine(nonExistentFsm, _testProcessingGroup));
        }

        /// <summary>
        /// Tests that destroying an FSM definition also removes any associated FSM handles.
        /// </summary>
        [Test]
        public void DestroyFiniteStateMachine_DestroysAssociatedHandles()
        {
            // Arrange
            FSM_API.Create.CreateInstance(_testFsmName, new Tests.Internal.FSMTestContext(), _testProcessingGroup);
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1));

            // Act
            FSM_API.Interaction.DestroyFiniteStateMachine(_testFsmName, _testProcessingGroup);

            // Assert
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0), "All associated handles should be destroyed.");
            Assert.That(FSM_API.Internal.DoesFsmDefinitionExist(_testProcessingGroup, _testFsmName), Is.False, "The FSM definition should be removed.");
        }
    }
}

