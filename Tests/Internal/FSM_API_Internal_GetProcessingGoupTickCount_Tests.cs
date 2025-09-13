using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API.Tests.Internal;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Internal_GetProcessingGoupTickCount_Tests
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
        /// Tests that GetProcessingGoupTickCount returns 0 for a new group.
        /// </summary>
        [Test]
        public void GetProcessingGoupTickCount_ReturnsZeroForNewGroup()
        {
            // Act
            var tickCount = FSM_API.Internal.GetProcessingGroupTickCount(GroupName);

            // Assert
            Assert.That(tickCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that GetProcessingGoupTickCount returns 0 for a non-existent group.
        /// </summary>
        [Test]
        public void GetProcessingGoupTickCount_ReturnsZeroForNonExistentGroup()
        {
            // Act
            var tickCount = FSM_API.Internal.GetProcessingGroupTickCount("NonExistentGroup");

            // Assert
            Assert.That(tickCount, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetProcessingGroupTickCount_ValidGroup_ReturnsCorrectCount()
        {
            // Arrange
            FSM_API.Create.CreateProcessingGroup("TestGroup");

            Helper_CreateTestFSM();
            Helper_CreateTestHandle();
            FSM_API.Interaction.Update("TestGroup");

            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetProcessingGroupTickCount_InvalidGroup_ReturnsZero()
        {
            // Arrange
            FSM_API.Create.CreateProcessingGroup("TestGroup");
            Helper_CreateTestFSM();
            Helper_CreateTestHandle();
            FSM_API.Interaction.Update("TestGroup");
            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("InvalidGroup"), Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetProcessingGroupTickCount_TracksMultipleTicks()
        {
            FSM_API.Create.CreateProcessingGroup("TestGroup1");
            FSM_API.Create.CreateProcessingGroup("TestGroup2");
            FSM_API.Create.CreateProcessingGroup("TestGroup3");

            Helper_CreateTestFSM("TestFSM1", "TestGroup1");
            Helper_CreateTestFSM("TestFSM2", "TestGroup2");
            Helper_CreateTestFSM("TestFSM3", "TestGroup3");

            Helper_CreateTestHandle("TestFSM1", "TestGroup1");
            Helper_CreateTestHandle("TestFSM2", "TestGroup2");
            Helper_CreateTestHandle("TestFSM3", "TestGroup3");

            //Tick everything once
            FSM_API.Interaction.Update("TestGroup1");
            FSM_API.Interaction.Update("TestGroup2");
            FSM_API.Interaction.Update("TestGroup3");
            //Tick 2 and 3 again
            FSM_API.Interaction.Update("TestGroup2");
            FSM_API.Interaction.Update("TestGroup3");
            //Tick 3 again
            FSM_API.Interaction.Update("TestGroup3");

            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("TestGroup1"), Is.EqualTo(1));
            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("TestGroup2"), Is.EqualTo(2));
            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("TestGroup3"), Is.EqualTo(3));

        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetPocessingGroupTickCount_TicksWhenOnlyFSMDefined()
        {
            FSM_API.Create.CreateProcessingGroup("TestGroup");
            Helper_CreateTestFSM("TetFSM", "TestGroup");

            FSM_API.Interaction.Update("TestGroup");
            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetPocessingGroupTickCount_TicksWhenFSM_and_HandleExist()
        {
            FSM_API.Create.CreateProcessingGroup("TestGroup");
            Helper_CreateTestFSM("TetFSM", "TestGroup");
            Helper_CreateTestHandle("TetFSM", "TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetPocessingGroupTickCount_TicksWhenFSM_and_Handle_DoNot_Exist()
        {
            FSM_API.Create.CreateProcessingGroup("TestGroup");
            
            FSM_API.Interaction.Update("TestGroup");
            Assert.That(FSM_API.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }

        private void Helper_CreateTestHandle(string fsmName = "TestFSM", string processingGroup = "TestGroup")
        {
            FSM_API.Create.CreateInstance(fsmName, new FSMTestContext(), processingGroup);
        }

        private void Helper_CreateTestFSM(string fsmName = "TestFSM", string processingGroup = "TestGroup")
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
                .BuildDefinition();
        }
    }
}
