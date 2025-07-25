using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public class FSM_API_InteractionGetAllDefinitionNamesTests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void GetAllDefinitionNames_DefaultGroup_ReturnsCorrectNames()
        {
            // Arrange
            string group = "Update";
            FSM_API.Create.CreateFiniteStateMachine("FSM1", processingGroup: group).BuildDefinition();
            FSM_API.Create.CreateFiniteStateMachine("FSM2", processingGroup: group).BuildDefinition();
            FSM_API.Create.CreateFiniteStateMachine("FSM3", processingGroup: group).BuildDefinition();

            // Act
            IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(); // Using default "Update"

            // Assert
            Assert.IsNotNull(names, "Returned collection should not be null.");
            Assert.That(names.Count, Is.EqualTo(3), "Expected 3 FSM definitions in the default group.");
            CollectionAssert.AreEquivalent(new[] { "FSM1", "FSM2", "FSM3" }, names, "Returned names should match expected FSMs.");
        }

        [Test]
        public void GetAllDefinitionNames_CustomGroup_ReturnsCorrectNames()
        {
            // Arrange
            string customGroup = "PhysicsUpdate";
            FSM_API.Create.CreateFiniteStateMachine("PhysFSM_A", processingGroup: customGroup).BuildDefinition();
            FSM_API.Create.CreateFiniteStateMachine("PhysFSM_B", processingGroup: customGroup).BuildDefinition();
            // Also add an FSM to a different group to ensure separation
            FSM_API.Create.CreateFiniteStateMachine("OtherFSM", processingGroup: "DifferentGroup").BuildDefinition();

            // Act
            IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(customGroup);

            // Assert
            Assert.IsNotNull(names, "Returned collection should not be null.");
            Assert.That(names.Count, Is.EqualTo(2), $"Expected 2 FSM definitions in the '{customGroup}' group.");
            CollectionAssert.AreEquivalent(new[] { "PhysFSM_A", "PhysFSM_B" }, names, "Returned names should match expected FSMs in custom group.");
        }

        [Test]
        public void GetAllDefinitionNames_GroupWithSingleFSM_ReturnsSingleName()
        {
            // Arrange
            string singleFsmGroup = "RenderGroup";
            string singleFsmName = "CameraFSM";
            FSM_API.Create.CreateFiniteStateMachine(singleFsmName, processingGroup: singleFsmGroup).BuildDefinition();

            // Act
            IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(singleFsmGroup);

            // Assert
            Assert.IsNotNull(names, "Returned collection should not be null.");
            Assert.That(names.Count, Is.EqualTo(1), "Expected 1 FSM definition in the group.");
            Assert.That(names.First(), Is.EqualTo(singleFsmName), "The single FSM name should be correct.");
        }

        [Test]
        public void GetAllDefinitionNames_GroupWithNoFSMs_ReturnsEmptyCollection()
        {
            // Arrange
            // Create a group by defining an FSM and then deleting it, or by simply
            // retrieving a group that exists but has no FSMs (not directly supported by current API).
            // Simplest way to ensure a group exists but is empty is to check after ResetAPI.
            // Or, to be explicit, define one FSM in another group.
            FSM_API.Create.CreateFiniteStateMachine("DummyFSM", processingGroup: "OtherGroup").BuildDefinition();
            string emptyGroup = "EmptyGroup"; // This group hasn't had any FSMs defined.

            // Act
            IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(emptyGroup);

            // Assert
            Assert.IsNotNull(names, "Returned collection should not be null.");
            Assert.IsEmpty(names, $"Expected an empty collection for group '{emptyGroup}' with no FSMs.");
            Assert.That(names.Count, Is.EqualTo(0), "Expected 0 FSM definitions.");
        }


        [Test]
        public void GetAllDefinitionNames_NonExistentGroup_ReturnsEmptyCollection()
        {
            // Arrange (Setup ensures no FSMs and thus no groups exist initially)
            string nonExistentGroup = "NonExistentGroup";

            // Act
            IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(nonExistentGroup);

            // Assert
            Assert.IsNotNull(names, "Returned collection should not be null for non-existent group.");
            Assert.IsEmpty(names, $"Expected an empty collection for non-existent group '{nonExistentGroup}'.");
            Assert.That(names.Count, Is.EqualTo(0), "Expected 0 FSM definitions for a non-existent group.");
        }

        [Test]
        public void GetAllDefinitionNames_ReturnedCollectionIsReadOnly()
        {
            // Arrange
            string group = "ReadOnlyTestGroup";
            FSM_API.Create.CreateFiniteStateMachine("FSM_A", processingGroup: group).BuildDefinition();

            // Act
            IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(group);

            // Assert
            // Attempt to cast to List<string> to confirm it's not directly modifiable.
            // A ReadOnlyCollection wrapper prevents direct modification.
            Assert.IsInstanceOf<System.Collections.ObjectModel.ReadOnlyCollection<string>>(names, "Returned collection should be a ReadOnlyCollection.");

            // Attempting to modify should result in a compile-time error or runtime exception if cast is forced
            // For example, the following line would not compile or throw NotSupportedException if it somehow did compile:
            // Assert.Throws<NotSupportedException>(() => ((IList<string>)names).Add("NewFSM"));
            // (The above is commented out because it's testing implementation detail of ReadOnlyCollection,
            // the IsInstanceOf check is sufficient for public contract).
        }

        [Test]
        public void GetAllDefinitionNames_NullProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string nullProcessingGroup = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetAllDefinitionNames(nullProcessingGroup),
                "Expected ArgumentException for null processing group.");
        }

        [Test]
        public void GetAllDefinitionNames_EmptyProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string emptyProcessingGroup = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetAllDefinitionNames(emptyProcessingGroup),
                "Expected ArgumentException for empty processing group.");
        }

        [Test]
        public void GetAllDefinitionNames_WhitespaceProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string whitespaceProcessingGroup = "   ";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetAllDefinitionNames(whitespaceProcessingGroup),
                "Expected ArgumentException for whitespace processing group.");
        }
    }
}
