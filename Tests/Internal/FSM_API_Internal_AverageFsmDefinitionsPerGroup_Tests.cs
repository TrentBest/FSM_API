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
    public class FSM_API_Internal_AverageFsmDefinitionsPerGroup_Tests
    {
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FsmApi.Internal.ResetAPI(true);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFSMDefinitionsPerGroup_ShouldReturnZero_WhenNoFSMDefinitionsExist()
        {
            // Arrange
            var expectedAverage = 0.0;
            // Act
            var actualAverage = FsmApi.Internal.AverageFsmDefinitionsPerGroup;
            // Assert
            Assert.That(actualAverage, Is.EqualTo(expectedAverage));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFSMDefinitionsPerGroup_ShouldReturnCorrectAverage_WhenFSMDefinitionsExist()
        {
            for (int i = 0; i < 100; i++)
            {
                Helper_CreateFSM($"TestFSM_{i}", "TestGroup");
            }

            Assert.That(FsmApi.Internal.AverageFsmDefinitionsPerGroup, Is.EqualTo(100.0));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFSMDefinitionsPerGroup_ShouldReturnCorrectAverage_WhenMultipleGroupsExist()
        {
            FsmApi.Create.CreateProcessingGroup("TestGroup1");
            FsmApi.Create.CreateProcessingGroup("TestGroup2");
            for (int i = 0; i < 50; i++)
            {
                Helper_CreateFSM($"TestFSM_Group1_{i}", "TestGroup1");

            }
            for (int i = 0; i < 25; i++)
            {
                Helper_CreateFSM($"TestFSM_Group2_{i}", "TestGroup2");
            }
            Assert.That(FsmApi.Internal.AverageFsmDefinitionsPerGroup, Is.EqualTo(37.5));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFSMDefinitionsPerGroup_ShouldReturnCorrectAverage_WhenHundredsGroupsExist()
        {
            for (int i = 0; i < 100; i++)
            {
                FsmApi.Create.CreateProcessingGroup($"TestGroup_{i}");
                for (int p = 0; p < i; p++)
                {
                    Helper_CreateFSM($"TestFSM_{p}_Group_{i}", $"TestGroup_{i}");
                }
            }
            Assert.That(FsmApi.Internal.AverageFsmDefinitionsPerGroup, Is.EqualTo(49.5));
        }

        /// <summary>
        /// Tests that the average is 0 when no FSM definitions exist.
        /// </summary>
        [Test]
        public void AverageFsmDefinitionsPerGroup_ReturnsZeroWhenNoDefinitionsExist()
        {
            // Act
            var average = FsmApi.Internal.AverageFsmDefinitionsPerGroup;

            // Assert
            Assert.That(average, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that the average is correct when a single FSM definition exists in one group.
        /// </summary>
        [Test]
        public void AverageFsmDefinitionsPerGroup_ReturnsCorrectAverageForSingleDefinition()
        {
            // Arrange
            FsmApi.Create.CreateFiniteStateMachine("FSM1").BuildDefinition();

            // Act
            var average = FsmApi.Internal.AverageFsmDefinitionsPerGroup;

            // Assert
            Assert.That(average, Is.EqualTo(1));
        }

        /// <summary>
        /// Tests that the average is correct when multiple FSM definitions exist in one group.
        /// </summary>
        [Test]
        public void AverageFsmDefinitionsPerGroup_ReturnsCorrectAverageForMultipleDefinitionsInOneGroup()
        {
            // Arrange
            FsmApi.Create.CreateFiniteStateMachine("FSM1").BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine("FSM2").BuildDefinition();

            // Act
            var average = FsmApi.Internal.AverageFsmDefinitionsPerGroup;

            // Assert
            Assert.That(average, Is.EqualTo(2));
        }

        /// <summary>
        /// Tests that the average is correct when FSM definitions are spread across multiple groups.
        /// </summary>
        [Test]
        public void AverageFsmDefinitionsPerGroup_ReturnsCorrectAverageForMultipleGroups()
        {
            // Arrange
            FsmApi.Create.CreateFiniteStateMachine("FSM1", processingGroup: "GroupA").BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine("FSM2", processingGroup: "GroupB").BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine("FSM3", processingGroup: "GroupB").BuildDefinition();

            // Act
            var average = FsmApi.Internal.AverageFsmDefinitionsPerGroup;

            // Assert
            // Total definitions: 3
            // Total groups: 2
            // Average = 3 / 2 = 1.5
            Assert.That(average, Is.EqualTo(1.5));
        }

        private void Helper_CreateFSM(string fsmName, string processGroup)
        {
            FsmApi.Create.CreateFiniteStateMachine(fsmName, 0, processGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}
