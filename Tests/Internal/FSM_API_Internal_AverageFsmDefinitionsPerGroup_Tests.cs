using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FSM_API.Internal;
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
            FSM_API.Internal.ResetAPI(true);
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
            var actualAverage = FSM_API.Internal.AverageFsmDefinitionsPerGroup;
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

            Assert.That(FSM_API.Internal.AverageFsmDefinitionsPerGroup, Is.EqualTo(100.0));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFSMDefinitionsPerGroup_ShouldReturnCorrectAverage_WhenMultipleGroupsExist()
        {
            FSM_API.Create.CreateProcessingGroup("TestGroup1");
            FSM_API.Create.CreateProcessingGroup("TestGroup2");
            for (int i = 0; i < 50; i++)
            {
                Helper_CreateFSM($"TestFSM_Group1_{i}", "TestGroup1");

            }
            for (int i = 0; i < 25; i++)
            {
                Helper_CreateFSM($"TestFSM_Group2_{i}", "TestGroup2");
            }
            Assert.That(FSM_API.Internal.AverageFsmDefinitionsPerGroup, Is.EqualTo(37.5));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFSMDefinitionsPerGroup_ShouldReturnCorrectAverage_WhenHundredsGroupsExist()
        {
            for (int i = 0; i < 100; i++)
            {
                FSM_API.Create.CreateProcessingGroup($"TestGroup_{i}");
                for (int p = 0; p < i; p++)
                {
                    Helper_CreateFSM($"TestFSM_{p}_Group_{i}", $"TestGroup_{i}");
                }
            }
            Assert.That(FSM_API.Internal.AverageFsmDefinitionsPerGroup, Is.EqualTo(49.5));
        }

        private void Helper_CreateFSM(string fsmName, string processGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, processGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}
