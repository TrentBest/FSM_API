using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FSM_API.Internal;
using static TheSingularityWorkshop.FSM_API.FSM_API.Interaction;
using TheSingularityWorkshop.FSM_API.Tests;


namespace TheSingularityWorkshop.FSM_API.Tests.Internal
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Internal_GetFsmHandleCountInGroup_Tests
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
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "Update");
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFSMHandleCountInGroup("Update"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsValidForTwoFSM_Test()
        {
            Helper_CreateTestFSM("TestFSM", "Update");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "Update");
            var handle2 = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "Update");
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFSMHandleCountInGroup("Update"), Is.EqualTo(2));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsValidForMultipleProcessingGroups_Test()
        {
            Helper_CreateTestFSM("TestFSM", "Update");
            Helper_CreateTestFSM("TestFSM", "FixedUpdate");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "Update");
            var handle2 = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "FixedUpdate");
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFSMHandleCountInGroup("Update"), Is.EqualTo(1));
            Assert.That(handle2, Is.Not.Null);
            Assert.That(handle2.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFSMHandleCountInGroup("FixedUpdate"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsZeroForNonExistentGroup_Test()
        {
            Assert.That(FSM_API.Internal.GetFSMHandleCountInGroup("NonExistentGroup"), Is.EqualTo(0));
        }


        private void Helper_CreateTestFSM(string fsmName, string processingGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}
