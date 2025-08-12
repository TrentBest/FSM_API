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
    public class FSM_API_InteractionGetAllDefinitionNamesTests
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
            Assert.That(names, Is.EqualTo(new[] { "FSM1", "FSM2", "FSM3" }), "Returned collection should not be null.");
            Assert.That(names.Count, Is.EqualTo(3), "Expected 3 FSM definitions in the default group.");
            
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void GetAllDefinitionNames_CustomGroup_ReturnsCorrectNames()
        //{
        //    // Arrange
        //    string customGroup = "PhysicsUpdate";
        //    FSM_API.Create.CreateFiniteStateMachine("PhysFSM_A", processingGroup: customGroup).BuildDefinition();
        //    FSM_API.Create.CreateFiniteStateMachine("PhysFSM_B", processingGroup: customGroup).BuildDefinition();
        //    // Also add an FSM to a different group to ensure separation
        //    FSM_API.Create.CreateFiniteStateMachine("OtherFSM", processingGroup: "DifferentGroup").BuildDefinition();

        //    // Act
        //    IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(customGroup);

        //    // Assert
        //    Assert.That(names, Is.True, "Returned collection should not be null.");
        //    Assert.That(names.Count, Is.EqualTo(2), $"Expected 2 FSM definitions in the '{customGroup}' group.");
        //    Assert.That(names, Is.EquivalentTo(new[] { "PhysFSM_A", "PhysFSM_B" }), 
        //        "Returned collection should contain the correct FSM names for the custom group.");
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void GetAllDefinitionNames_GroupWithSingleFSM_ReturnsSingleName()
        //{
        //    // Arrange
        //    string singleFsmGroup = "RenderGroup";
        //    string singleFsmName = "CameraFSM";
        //    FSM_API.Create.CreateFiniteStateMachine(singleFsmName, processingGroup: singleFsmGroup).BuildDefinition();

        //    // Act
        //    IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(singleFsmGroup);

        //    // Assert
        //    Assert.That(names, Is.True, "Returned collection should not be null.");
        //    Assert.That(names.Count, Is.EqualTo(1), "Expected 1 FSM definition in the group.");
        //    Assert.That(names.First(), Is.EqualTo(singleFsmName), "The single FSM name should be correct.");
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void GetAllDefinitionNames_GroupWithNoFSMs_ReturnsEmptyCollection()
        //{
        //    // Arrange
        //    // Create a group by defining an FSM and then deleting it, or by simply
        //    // retrieving a group that exists but has no FSMs (not directly supported by current API).
        //    // Simplest way to ensure a group exists but is empty is to check after ResetAPI.
        //    // Or, to be explicit, define one FSM in another group.
        //    FSM_API.Create.CreateFiniteStateMachine("DummyFSM", processingGroup: "OtherGroup").BuildDefinition();
        //    string emptyGroup = "EmptyGroup"; // This group hasn't had any FSMs defined.

        //    // Act
        //    IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(emptyGroup);

        //    // Assert
        //    Assert.That(names, Is.True, "Returned collection should not be null.");
        //    Assert.That(names, Is.True, $"Expected an empty collection for group '{emptyGroup}' with no FSMs.");
        //    Assert.That(names.Count, Is.EqualTo(0), "Expected 0 FSM definitions.");
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void GetAllDefinitionNames_NonExistentGroup_ReturnsEmptyCollection()
        //{
        //    // Arrange (Setup ensures no FSMs and thus no groups exist initially)
        //    string nonExistentGroup = "NonExistentGroup";

        //    // Act
        //    IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(nonExistentGroup);

        //    // Assert
        //    Assert.That(names, Is.True, "Returned collection should not be null for non-existent group.");
        //    Assert.That(names, Is.True, $"Expected an empty collection for non-existent group '{nonExistentGroup}'.");
        //    Assert.That(names.Count, Is.EqualTo(0), "Expected 0 FSM definitions for a non-existent group.");
        //}

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllDefinitionNames_ReturnedCollectionIsReadOnly()
        {
            // Arrange
            string group = "ReadOnlyTestGroup";
            FSM_API.Create.CreateFiniteStateMachine("FSM_A", processingGroup: group).BuildDefinition();

            // Act
            IReadOnlyCollection<string> names = FSM_API.Interaction.GetAllDefinitionNames(group);

            // Assert
            // 1. Ensure it's not null.
            Assert.That(names, Is.Not.Null, "Returned collection should not be null.");

            // 2. Ensure it implements IReadOnlyCollection<string> (implicit from the return type, but good for clarity)
            // This assertion is often redundant if the method's signature already guarantees IReadOnlyCollection<string>.
            Assert.That(names, Is.InstanceOf<IReadOnlyCollection<string>>(), "Returned collection should implement IReadOnlyCollection.");

            // 3. More importantly: Ensure it's NOT a mutable collection type.
            // If it's a List<string> or similar, it could be cast back and modified.
            // This is the core of what you're trying to prevent.
            Assert.That(names, Is.Not.InstanceOf<List<string>>(), "Returned collection should not be a mutable List<string>.");
            Assert.That(names, Is.Not.InstanceOf<HashSet<string>>(), "Returned collection should not be a mutable HashSet<string>.");
            // Add other mutable collection types if applicable, e.g., Dictionary, if it were a different scenario.
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllDefinitionNames_NullProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string nullProcessingGroup = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetAllDefinitionNames(nullProcessingGroup),
                "Expected ArgumentException for null processing group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllDefinitionNames_EmptyProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string emptyProcessingGroup = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.GetAllDefinitionNames(emptyProcessingGroup),
                "Expected ArgumentException for empty processing group.");
        }

        /// <summary>
        /// 
        /// </summary>
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
