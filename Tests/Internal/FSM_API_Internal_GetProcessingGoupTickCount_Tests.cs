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
            FsmApi.Internal.ResetAPI(true);
            FsmApi.Create.CreateProcessingGroup(GroupName);
        }

        /// <summary>
        /// Tests that GetProcessingGoupTickCount returns 0 for a new group.
        /// </summary>
        [Test]
        public void GetProcessingGoupTickCount_ReturnsZeroForNewGroup()
        {
            // Act
            var tickCount = FsmApi.Internal.GetProcessingGroupTickCount(GroupName);

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
            var tickCount = FsmApi.Internal.GetProcessingGroupTickCount("NonExistentGroup");

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
            FsmApi.Create.CreateProcessingGroup("TestGroup");

            Helper_CreateTestFSM();
            Helper_CreateTestHandle();
            FsmApi.Interaction.Update("TestGroup");

            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetProcessingGroupTickCount_InvalidGroup_ReturnsZero()
        {
            // Arrange
            FsmApi.Create.CreateProcessingGroup("TestGroup");
            Helper_CreateTestFSM();
            Helper_CreateTestHandle();
            FsmApi.Interaction.Update("TestGroup");
            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("InvalidGroup"), Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetProcessingGroupTickCount_TracksMultipleTicks()
        {
            FsmApi.Create.CreateProcessingGroup("TestGroup1");
            FsmApi.Create.CreateProcessingGroup("TestGroup2");
            FsmApi.Create.CreateProcessingGroup("TestGroup3");

            Helper_CreateTestFSM("TestFSM1", "TestGroup1");
            Helper_CreateTestFSM("TestFSM2", "TestGroup2");
            Helper_CreateTestFSM("TestFSM3", "TestGroup3");

            Helper_CreateTestHandle("TestFSM1", "TestGroup1");
            Helper_CreateTestHandle("TestFSM2", "TestGroup2");
            Helper_CreateTestHandle("TestFSM3", "TestGroup3");

            //Tick everything once
            FsmApi.Interaction.Update("TestGroup1");
            FsmApi.Interaction.Update("TestGroup2");
            FsmApi.Interaction.Update("TestGroup3");
            //Tick 2 and 3 again
            FsmApi.Interaction.Update("TestGroup2");
            FsmApi.Interaction.Update("TestGroup3");
            //Tick 3 again
            FsmApi.Interaction.Update("TestGroup3");

            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("TestGroup1"), Is.EqualTo(1));
            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("TestGroup2"), Is.EqualTo(2));
            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("TestGroup3"), Is.EqualTo(3));

        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetPocessingGroupTickCount_TicksWhenOnlyFSMDefined()
        {
            FsmApi.Create.CreateProcessingGroup("TestGroup");
            Helper_CreateTestFSM("TetFSM", "TestGroup");

            FsmApi.Interaction.Update("TestGroup");
            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetPocessingGroupTickCount_TicksWhenFSM_and_HandleExist()
        {
            FsmApi.Create.CreateProcessingGroup("TestGroup");
            Helper_CreateTestFSM("TetFSM", "TestGroup");
            Helper_CreateTestHandle("TetFSM", "TestGroup");
            FsmApi.Interaction.Update("TestGroup");
            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetPocessingGroupTickCount_TicksWhenFSM_and_Handle_DoNot_Exist()
        {
            FsmApi.Create.CreateProcessingGroup("TestGroup");
            
            FsmApi.Interaction.Update("TestGroup");
            Assert.That(FsmApi.Internal.GetProcessingGroupTickCount("TestGroup"), Is.EqualTo(1));
        }

        private void Helper_CreateTestHandle(string fsmName = "TestFSM", string processingGroup = "TestGroup")
        {
            FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), processingGroup);
        }

        private void Helper_CreateTestFSM(string fsmName = "TestFSM", string processingGroup = "TestGroup")
        {
            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
                .BuildDefinition();
        }
    }
}
