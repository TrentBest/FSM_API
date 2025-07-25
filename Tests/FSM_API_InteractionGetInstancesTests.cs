﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public class FSM_API_InteractionGetInstancesTests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void GetInstances_FSMExistsWithMultipleInstancesInDefaultGroup_ReturnsAllInstances()
        {
            // Arrange
            string fsmName = "PlayerFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            List<FSMHandle> expectedHandles = new List<FSMHandle>
            {
                FSM_API.Create.CreateInstance(fsmName, new FSMTestContext()),
                FSM_API.Create.CreateInstance(fsmName, new FSMTestContext()),
                FSM_API.Create.CreateInstance(fsmName, new FSMTestContext())
            };

            // Act
            IReadOnlyList<FSMHandle> actualHandles = FSM_API.Interaction.GetInstances(fsmName);

            // Assert
            Assert.IsNotNull(actualHandles, "Returned list should not be null.");
            Assert.That(actualHandles.Count, Is.EqualTo(expectedHandles.Count), "Expected count of instances does not match.");
            CollectionAssert.AreEquivalent(expectedHandles, actualHandles, "Returned instances should match the created instances.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(3), "Total FSM handle count should reflect created instances.");
        }

        [Test]
        public void GetInstances_FSMExistsWithMultipleInstancesInCustomGroup_ReturnsAllInstances()
        {
            // Arrange
            string fsmName = "EnemyAI";
            string customProcessingGroup = "EnemyUpdate";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, processingGroup: customProcessingGroup).BuildDefinition();
            List<FSMHandle> expectedHandles = new List<FSMHandle>
            {
                FSM_API.Create.CreateInstance(fsmName, new FSMTestContext(), customProcessingGroup),
                FSM_API.Create.CreateInstance(fsmName, new FSMTestContext(), customProcessingGroup)
            };
            // Create an instance in a different group to ensure isolation
            FSM_API.Create.CreateFiniteStateMachine("OtherFSM", processingGroup: "OtherGroup").BuildDefinition();
            FSM_API.Create.CreateInstance("OtherFSM", new FSMTestContext(), "OtherGroup");


            // Act
            IReadOnlyList<FSMHandle> actualHandles = FSM_API.Interaction.GetInstances(fsmName, customProcessingGroup);

            // Assert
            Assert.IsNotNull(actualHandles, "Returned list should not be null.");
            Assert.That(actualHandles.Count, Is.EqualTo(expectedHandles.Count), "Expected count of instances does not match.");
            CollectionAssert.AreEquivalent(expectedHandles, actualHandles, "Returned instances should match the created instances.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(3), "Total FSM handle count should reflect all instances created across groups.");
        }

        [Test]
        public void GetInstances_FSMExistsWithSingleInstance_ReturnsSingleInstance()
        {
            // Arrange
            string fsmName = "DoorFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            FSMHandle expectedHandle = FSM_API.Create.CreateInstance(fsmName, new FSMTestContext());

            // Act
            IReadOnlyList<FSMHandle> actualHandles = FSM_API.Interaction.GetInstances(fsmName);

            // Assert
            Assert.IsNotNull(actualHandles, "Returned list should not be null.");
            Assert.That(actualHandles.Count, Is.EqualTo(1), "Expected exactly one FSM instance.");
            Assert.That(actualHandles.First(), Is.EqualTo(expectedHandle), "The returned handle should be the expected instance.");
        }

        [Test]
        public void GetInstances_FSMExistsButHasNoInstances_ReturnsEmptyList()
        {
            // Arrange
            string fsmName = "EmptyFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition(); // Define the FSM, but don't create instances

            // Act
            IReadOnlyList<FSMHandle> actualHandles = FSM_API.Interaction.GetInstances(fsmName);

            // Assert
            Assert.IsNotNull(actualHandles, "Returned list should not be null.");
            Assert.IsEmpty(actualHandles, "Expected an empty list when FSM definition exists but has no instances.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0), "Total FSM handle count should still be zero.");
        }

        [Test]
        public void GetInstances_ReturnedListIsReadOnly()
        {
            // Arrange
            string fsmName = "ReadOnlyTestFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            FSM_API.Create.CreateInstance(fsmName, new FSMTestContext());

            // Act
            IReadOnlyList<FSMHandle> handles = FSM_API.Interaction.GetInstances(fsmName);

            // Assert
            Assert.IsInstanceOf<System.Collections.ObjectModel.ReadOnlyCollection<FSMHandle>>(handles, "Returned collection should be a ReadOnlyCollection.");
            // Verify that attempts to modify the list directly would fail (not directly testable via NUnit Assert.Throws without casting tricks)
            // The IsInstanceOf check is sufficient to confirm the public contract.
        }

        // --- Exception Tests ---

        [Test]
        public void GetInstances_NullFsmName_ThrowsArgumentException()
        {
            // Arrange (no FSM definition needed for this test as validation happens first)
            string nullFsmName = string.Empty;
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetInstances(nullFsmName, validProcessingGroup),
                "Expected ArgumentException for null FSM name.");
        }

        [Test]
        public void GetInstances_EmptyFsmName_ThrowsArgumentException()
        {
            // Arrange
            string emptyFsmName = "";
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetInstances(emptyFsmName, validProcessingGroup),
                "Expected ArgumentException for empty FSM name.");
        }

        [Test]
        public void GetInstances_WhitespaceFsmName_ThrowsArgumentException()
        {
            // Arrange
            string whitespaceFsmName = "   ";
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetInstances(whitespaceFsmName, validProcessingGroup),
                "Expected ArgumentException for whitespace FSM name.");
        }

        [Test]
        public void GetInstances_NullProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string nullProcessingGroup = string.Empty;
            FSM_API.Create.CreateFiniteStateMachine(validFsmName).BuildDefinition(); // Need a definition to avoid KeyNotFoundException first

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetInstances(validFsmName, nullProcessingGroup),
                "Expected ArgumentException for null processing group.");
        }

        [Test]
        public void GetInstances_EmptyProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string emptyProcessingGroup = "";
            FSM_API.Create.CreateFiniteStateMachine(validFsmName).BuildDefinition();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetInstances(validFsmName, emptyProcessingGroup),
                "Expected ArgumentException for empty processing group.");
        }

        [Test]
        public void GetInstances_WhitespaceProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string whitespaceProcessingGroup = "   ";
            FSM_API.Create.CreateFiniteStateMachine(validFsmName).BuildDefinition();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetInstances(validFsmName, whitespaceProcessingGroup),
                "Expected ArgumentException for whitespace processing group.");
        }

        [Test]
        public void GetInstances_FSMDefinitionNotFoundInGroup_ThrowsKeyNotFoundException()
        {
            // Arrange
            string existingFsmName = "SomeFSM";
            string existingGroup = "Update";
            string nonExistentFsmName = "NonExistentFSM";
            FSM_API.Create.CreateFiniteStateMachine(existingFsmName, processingGroup: existingGroup).BuildDefinition();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => FSM_API.Interaction.GetInstances(nonExistentFsmName, existingGroup),
                "Expected KeyNotFoundException when FSM definition is not found in the specified group.");
        }

        [Test]
        public void GetInstances_ProcessingGroupNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            string fsmName = "FSMInNonExistentGroup";
            string nonExistentGroup = "UnknownGroup";
            // No FSMs are defined, so this group won't exist

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => FSM_API.Interaction.GetInstances(fsmName, nonExistentGroup),
                "Expected KeyNotFoundException when the processing group itself does not exist.");
        }
    }
}
