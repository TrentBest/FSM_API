using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FsmApi.Internal;
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
            FsmApi.Internal.ResetAPI(true);
            FsmApi.Create.CreateProcessingGroup(GroupName);
        }

        /// <summary>
        /// Tests that a processing group can be successfully removed.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_Succeeds()
        {
            // Arrange
            Assert.That(FsmApi.Internal.GetProcessingGroupNames().Contains(GroupName), Is.True);

            // Act
            FsmApi.Interaction.RemoveProcessingGroup(GroupName);

            // Assert
            Assert.That(FsmApi.Internal.GetProcessingGroupNames().Contains(GroupName), Is.False);
            Assert.That(FsmApi.Internal.ProcessingGroupCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that attempting to remove a non-existent group does not throw an exception.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_NonExistentGroup_DoesNotThrow()
        {
            // Arrange
            string nonExistentGroup = "NonExistentGroup";
            Assert.That(FsmApi.Internal.GetProcessingGroupNames().Contains(nonExistentGroup), Is.False);

            // Act & Assert
            Assert.DoesNotThrow(() => FsmApi.Interaction.RemoveProcessingGroup(nonExistentGroup));
        }

        /// <summary>
        /// Tests that a group containing FSM definitions is removed, and the definitions are destroyed.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_WithDefinitions_DestroysDefinitionsAndSucceeds()
        {
            // Arrange
            FsmApi.Create.CreateFiniteStateMachine("TestFSM", processingGroup: GroupName).BuildDefinition();
            Assert.That(FsmApi.Internal.GetFsmDefinitionCountInGroup(GroupName), Is.EqualTo(1));

            // Act
            FsmApi.Interaction.RemoveProcessingGroup(GroupName);

            // Assert
            Assert.That(FsmApi.Internal.GetProcessingGroupNames().Contains(GroupName), Is.False);
            Assert.That(FsmApi.Internal.TotalFsmDefinitionCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that a group containing FSM instances is removed, and the instances are destroyed.
        /// </summary>
        [Test]
        public void RemoveProcessingGroup_WithInstances_DestroysInstancesAndSucceeds()
        {
            // Arrange
            string fsmName = "TestFSM";
            FsmApi.Create.CreateFiniteStateMachine(fsmName, processingGroup: GroupName).BuildDefinition();
            FsmApi.Create.CreateInstance(fsmName, new Tests.Internal.FSMTestContext(), GroupName);
            Assert.That(FsmApi.Internal.GetFsmDefinitionCountInGroup(GroupName), Is.EqualTo(1));

            // Act
            FsmApi.Interaction.RemoveProcessingGroup(GroupName);

            // Assert
            Assert.That(FsmApi.Internal.GetProcessingGroupNames().Contains(GroupName), Is.False);
            Assert.That(FsmApi.Internal.TotalFsmHandleCount, Is.EqualTo(0));
        }
    }
}
