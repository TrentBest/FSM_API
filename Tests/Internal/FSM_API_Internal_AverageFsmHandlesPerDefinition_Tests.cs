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
    public class FSM_API_Internal_AverageFsmHandlesPerDefinition_Tests
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
        public void AverageFsmHandlesPerDefinition_ShouldReturnZero_WhenNoFsmDefinitionsExist()
        {
            // Arrange
            var expectedAverage = 0.0;
            // Act
            var actualAverage = FsmApi.Internal.AverageFsmHandlesPerDefinition;
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
                FsmApi.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            }
            Assert.That(FsmApi.Internal.AverageFsmHandlesPerDefinition, Is.EqualTo(100.0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void AverageFsmHandlesPerDefinition_ShouldReturnCorrectAverage_WhenMultipleGroupsExist()
        {
            FsmApi.Create.CreateProcessingGroup("TestGroup1");
            FsmApi.Create.CreateProcessingGroup("TestGroup2");
            Helper_CreateFSM($"TestFSM1", "TestGroup1");
            Helper_CreateFSM($"TestFSM2", "TestGroup2");
            for (int i = 0; i < 50; i++)
            {
                FsmApi.Create.CreateInstance("TestFSM1", new FSMTestContext(), "TestGroup1");
                FsmApi.Create.CreateInstance("TestFSM2", new FSMTestContext(), "TestGroup2");
            }
            Assert.That(FsmApi.Internal.AverageFsmHandlesPerDefinition, Is.EqualTo(50.0));
        }

        private void Helper_CreateFSM(string fsmName, string processingGroup)
        {
            FsmApi.Create.CreateFiniteStateMachine(fsmName, 0, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}
