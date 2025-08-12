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
    public class FSM_API_Internal_GetFsmDefinitionCountInGroup_Tests
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
        public void CountIsValidForOneFSM_Test()
        {
           Helper_CreateTestFSM("TestFSM", "Update");
            //TestContext tc = new TestContext();
            //var handle = FSM_API.Create.CreateInstance("TestFSM", tc, "Update");
            //Assert.That(handle, Is.Not.Null);
            //Assert.That(handle.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup("Update"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsValidForTwoFSM_Test()
        {
            Helper_CreateTestFSM("TestFSM", "Update");
            Helper_CreateTestFSM("TestFSM2", "Update");

            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup("Update"), Is.EqualTo(2));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsValidForTwoFSMsInDifferentGroups_Test()
        {
            Helper_CreateTestFSM("TestFSM", "Update");
            Helper_CreateTestFSM("TestFSM2", "FixedUpdate");
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup("Update"), Is.EqualTo(1));
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup("FixedUpdate"), Is.EqualTo(1));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsValidForTwoHundredFSMsInDifferentGroups_Test()
        {
            string sourceString = "Test";
            for (int i = 0; i < 200; i++)
            {
                string fsmName = sourceString + i.ToString();
                string processingGroup = i % 2 == 0 ? "Update" : "FixedUpdate";
                Helper_CreateTestFSM(fsmName, processingGroup);
            }
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup("Update"), Is.EqualTo(100));
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup("FixedUpdate"), Is.EqualTo(100));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsZeroForNonExistentGroup_Test()
        {
            Assert.That(FSM_API.Internal.GetFsmDefinitionCountInGroup("NonExistentGroup"), Is.EqualTo(0));
        }

        private void Helper_CreateTestFSM(string fsmName, string processingGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}
