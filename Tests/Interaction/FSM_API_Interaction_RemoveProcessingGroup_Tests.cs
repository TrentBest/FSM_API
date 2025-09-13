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
    public class FSM_API_Interaction_RemoveProcessingGroup_Tests
    {
        private const string GroupName = "TestGroup";
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
            FSM_API.Create.CreateProcessingGroup(GroupName);
        }

        /// <summary>
        /// Tests that a processing group can be successfully removed.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_Succeeds()
        {
            // Arrange
            Assert.That(FSM_API.Internal.GetProcessingGroupNames().Contains(GroupName), Is.True);

            // Act
            FSM_API.Interaction.RemoveProcessingGroup(GroupName);

            // Assert
            Assert.That(FSM_API.Internal.GetProcessingGroupNames().Contains(GroupName), Is.False);
            Assert.That(FSM_API.Internal.ProcessingGroupCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that attempting to remove a non-existent group does not throw an exception.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_NonExistentGroup_DoesNotThrow()
        {
            // Arrange
            string nonExistentGroup = "NonExistentGroup";
            Assert.That(FSM_API.Internal.GetProcessingGroupNames().Contains(nonExistentGroup), Is.False);

            // Act & Assert
            Assert.DoesNotThrow(() => FSM_API.Interaction.RemoveProcessingGroup(nonExistentGroup));
        }

        /// <summary>
        /// Tests that a group containing FSM definitions is removed, and the definitions are destroyed.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_WithDefinitions_DestroysDefinitionsAndSucceeds()
        {
            // Arrange
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", processingGroup: GroupName).BuildDefinition();
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup(GroupName), Is.EqualTo(1));

            // Act
            FSM_API.Interaction.RemoveProcessingGroup(GroupName);

            // Assert
            Assert.That(FSM_API.Internal.GetProcessingGroupNames().Contains(GroupName), Is.False);
            Assert.That(FSM_API.Internal.TotalFsmDefinitionCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that a group containing FSM instances is removed, and the instances are destroyed.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_WithInstances_DestroysInstancesAndSucceeds()
        {
            // Arrange
            string fsmName = "TestFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, processingGroup: GroupName).BuildDefinition();
            FSM_API.Create.CreateInstance(fsmName, new Tests.Internal.FSMTestContext(), GroupName);
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup(GroupName), Is.EqualTo(1));

            // Act
            FSM_API.Interaction.RemoveProcessingGroup(GroupName);

            // Assert
            Assert.That(FSM_API.Internal.GetProcessingGroupNames().Contains(GroupName), Is.False);
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0));
        }
    }
}
