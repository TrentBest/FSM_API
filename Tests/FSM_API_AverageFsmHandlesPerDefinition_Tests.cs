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
    public class FSM_API_AverageFsmHandlesPerDefinition_Tests
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
        public void AverageFsmHandlesPerDefinition_ShouldReturnZero_WhenNoFsmDefinitionsExist()
        {
            // Arrange
            var expectedAverage = 0.0;
            // Act
            var actualAverage = FSM_API.Internal.AverageFsmHandlesPerDefinition;
            // Assert
            Assert.That(actualAverage, Is.EqualTo(expectedAverage));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFsmHandlesPerDefinition_ShouldReturnCorrectAverage_WhenFsmDefinitionsExist()
        {
            Helper_CreateFSM($"TestFSM", "TestGroup");
            for (int i = 0; i < 100; i++)
            {
                FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "TestGroup");
            }
            Assert.That(FSM_API.Internal.AverageFsmHandlesPerDefinition, Is.EqualTo(100.0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFsmHandlesPerDefinition_ShouldReturnCorrectAverage_WhenMultipleGroupsExist()
        {
            FSM_API.Create.CreateProcessingGroup("TestGroup1");
            FSM_API.Create.CreateProcessingGroup("TestGroup2");
            Helper_CreateFSM($"TestFSM1", "TestGroup1");
            Helper_CreateFSM($"TestFSM2", "TestGroup2");
            for (int i = 0; i < 50; i++)
            {
                FSM_API.Create.CreateInstance("TestFSM1", new TestContext(), "TestGroup1");
                FSM_API.Create.CreateInstance("TestFSM2", new TestContext(), "TestGroup2");
            }
            Assert.That(FSM_API.Internal.AverageFsmHandlesPerDefinition, Is.EqualTo(50.0));
        }

        private void Helper_CreateFSM(string fsmName, string processingGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}
