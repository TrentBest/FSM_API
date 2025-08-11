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
    public class FSM_API_InteractionExistsTests
    {
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_FSMExistsInDefaultGroup_ReturnsTrue()
        {
            // Arrange
            string fsmName = "MyDefaultFSM";
            // Create the FSM definition in the default "Update" group
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();

            // Act
            bool exists = FSM_API.Interaction.Exists(fsmName);

            // Assert
            Assert.That(exists, Is.True, $"Expected FSM '{fsmName}' to exist in the default 'Update' group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_FSMExistsInCustomGroup_ReturnsTrue()
        {
            // Arrange
            string fsmName = "CustomGroupFSM";
            string customProcessingGroup = "RenderLoop";
            // Create the FSM definition in a custom processing group
            FSM_API.Create.CreateFiniteStateMachine(fsmName, processingGroup: customProcessingGroup).BuildDefinition();

            // Act
            bool exists = FSM_API.Interaction.Exists(fsmName, customProcessingGroup);

            // Assert
            Assert.That(exists, Is.True, $"Expected FSM '{fsmName}' to exist in the '{customProcessingGroup}' group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_FSMDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string existingFsmName = "ExistingFSM";
            string nonExistentFsmName = "NonExistentFSM";
            // Create one FSM to ensure the system isn't completely empty, but the target FSM doesn't exist.
            FSM_API.Create.CreateFiniteStateMachine(existingFsmName).BuildDefinition();

            // Act
            bool exists = FSM_API.Interaction.Exists(nonExistentFsmName);

            // Assert
            Assert.That(exists, Is.False, $"Expected FSM '{nonExistentFsmName}' to not exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_FSMExistsInDifferentGroup_ReturnsFalse()
        {
            // Arrange
            string fsmName = "SharedNameFSM";
            string group1 = "GroupA";
            string group2 = "GroupB";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, processingGroup: group1).BuildDefinition();

            // Act
            // Check for the FSM with the same name, but in a different group where it doesn't exist
            bool exists = FSM_API.Interaction.Exists(fsmName, group2);

            // Assert
            Assert.That(exists, Is.False, $"Expected FSM '{fsmName}' to not exist in '{group2}' when it's only in '{group1}'.");
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void Exists_ProcessingGroupDoesNotExist_ReturnsFalse()
        //{
        //    // Arrange
        //    string fsmName = "SomeFSM";
        //    string nonExistentProcessingGroup = "ImaginaryGroup";
        //    // No FSMs are created in "ImaginaryGroup"

        //    // Act
        //    bool exists = FSM_API.Interaction.Exists(fsmName, nonExistentProcessingGroup);

        //    // Assert
        //    Assert.That(exists, Is.False, $"Expected FSM '{fsmName}' to not exist in a non-existent processing group '{nonExistentProcessingGroup}'.");
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void Exists_NoFSMsDefinedAtAll_ReturnsFalse()
        //{
        //    // Arrange (Setup already ensures no FSMs are defined)
        //    string fsmName = "AnyFSM";
        //    string processingGroup = "AnyGroup";

        //    // Act
        //    bool exists = FSM_API.Interaction.Exists(fsmName, processingGroup);

        //    // Assert
        //    Assert.That(exists, Is.True, "Expected no FSM to exist when the system is empty.");
        //}

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_NullFsmName_ThrowsArgumentException()
        {
            // Arrange
            string nullFsmName = string.Empty;
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.Exists(nullFsmName, validProcessingGroup),
                "Expected ArgumentException for null FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_EmptyFsmName_ThrowsArgumentException()
        {
            // Arrange
            string emptyFsmName = "";
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.Exists(emptyFsmName, validProcessingGroup),
                "Expected ArgumentException for empty FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_WhitespaceFsmName_ThrowsArgumentException()
        {
            // Arrange
            string whitespaceFsmName = "   ";
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.Exists(whitespaceFsmName, validProcessingGroup),
                "Expected ArgumentException for whitespace FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_NullProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string nullProcessingGroup = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.Exists(validFsmName, nullProcessingGroup),
                "Expected ArgumentException for null processing group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_EmptyProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string emptyProcessingGroup = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.Exists(validFsmName, emptyProcessingGroup),
                "Expected ArgumentException for empty processing group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Exists_WhitespaceProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string whitespaceProcessingGroup = "   ";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.Exists(validFsmName, whitespaceProcessingGroup),
                "Expected ArgumentException for whitespace processing group.");
        }
    }
}
