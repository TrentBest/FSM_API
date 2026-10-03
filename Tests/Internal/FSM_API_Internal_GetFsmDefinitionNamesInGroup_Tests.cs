using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FsmApi.Internal;
using TheSingularityWorkshop.FSM_API.Tests;


namespace TheSingularityWorkshop.FSM_API.Tests.Internal
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Internal_GetFsmDefinitionNamesInGroup_Tests
    {
        private const string ProcessGroup = "Update";

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FsmApi.Internal.ResetAPI(true);
        }

        /// <summary>
        /// Tests that GetFsmDefinitionNamesInGroup returns an empty list when no FSM definitions exist.
        /// </summary>
        [Test]
        public void GetFsmDefinitionNamesInGroup_ReturnsEmptyListWhenNoDefinitionsExist()
        {
            // Act
            var names = FsmApi.Internal.GetFsmDefinitionNamesInGroup(ProcessGroup);

            // Assert
            Assert.That(names, Is.Empty);
        }

        /// <summary>
        /// Tests that GetFsmDefinitionNamesInGroup returns the correct names for a single group.
        /// </summary>
        [Test]
        public void GetFsmDefinitionNamesInGroup_ReturnsCorrectNamesForSingleGroup()
        {
            // Arrange
            FsmApi.Create.CreateFiniteStateMachine("FSM1", processingGroup: ProcessGroup).BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine("FSM2", processingGroup: ProcessGroup).BuildDefinition();

            // Act
            var names = FsmApi.Internal.GetFsmDefinitionNamesInGroup(ProcessGroup);

            // Assert
            Assert.That(names.Count(), Is.EqualTo(2));
            Assert.That(names.OrderBy(n => n), Is.EqualTo(new[] { "FSM1", "FSM2" }));
        }

        /// <summary>
        /// Tests that GetFsmDefinitionNamesInGroup returns an empty list for a non-existent group.
        /// </summary>
        [Test]
        public void GetFsmDefinitionNamesInGroup_ReturnsEmptyListForNonExistentGroup()
        {
            // Arrange
            FsmApi.Create.CreateFiniteStateMachine("FSM1", processingGroup: ProcessGroup).BuildDefinition();

            // Act
            var names = FsmApi.Internal.GetFsmDefinitionNamesInGroup("NonExistentGroup");

            // Assert
            Assert.That(names, Is.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFsmDefinitionNamesInGroup_Empty_Test()
        {
            var names = FsmApi.Internal.GetFsmDefinitionNamesInGroup("Update");
            Assert.That(names, Is.Not.Null, "GetFsmDefinitionNamesInGroup should not return null.");
            Assert.That(names, Is.Empty, "GetFsmDefinitionNamesInGroup should return an empty list when no FSMs are defined in the group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFsmDefinitionNamesInGroup_WithFSMs_Test()
        {
            string fsmName = "TestFSM";
            string groupName = "Update";
            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            var names = FsmApi.Internal.GetFsmDefinitionNamesInGroup(groupName).ToArray();
            Assert.That(names, Is.Not.Null, "GetFsmDefinitionNamesInGroup should not return null.");
            Assert.That(names.Count, Is.EqualTo(1), "GetFsmDefinitionNamesInGroup should return a list with one FSM name.");
            Assert.That(names[0], Is.EqualTo(fsmName), "GetFsmDefinitionNamesInGroup should return the correct FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFsmDefinitionNamesInGroup_WithMultipleFSMs_Test()
        {
            string fsmName1 = "TestFSM1";
            string fsmName2 = "TestFSM2";
            string groupName = "Update";
            FsmApi.Create.CreateFiniteStateMachine(fsmName1, -1, groupName).BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine(fsmName2, -1, groupName).BuildDefinition();
            var names = FsmApi.Internal.GetFsmDefinitionNamesInGroup(groupName).ToArray();
            Assert.That(names, Is.Not.Null, "GetFsmDefinitionNamesInGroup should not return null.");
            Assert.That(names.Count, Is.EqualTo(2), "GetFsmDefinitionNamesInGroup should return a list with two FSM names.");
            Assert.That(names.Contains(fsmName1), Is.True, "GetFsmDefinitionNamesInGroup should contain the first FSM name.");
            Assert.That(names.Contains(fsmName2), Is.True, "GetFsmDefinitionNamesInGroup should contain the second FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFsmDefinitionNamesInGroup_InvalidGroup_Test()
        {
            var names = FsmApi.Internal.GetFsmDefinitionNamesInGroup("InvalidGroup");
            Assert.That(names, Is.Not.Null, "GetFsmDefinitionNamesInGroup should not return null for an invalid group.");
            Assert.That(names, Is.Empty, "GetFsmDefinitionNamesInGroup should return an empty list when the group is invalid.");
        }
    }
}
