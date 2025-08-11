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
    public class FSM_API_GetFsmHandleCountInGroup_Tests
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
            var handle = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "Update");
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFsmHandleCountInGroup("Update"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsValidForTwoFSM_Test()
        {
            Helper_CreateTestFSM("TestFSM", "Update");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "Update");
            var handle2 = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "Update");
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFsmHandleCountInGroup("Update"), Is.EqualTo(2));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsValidForMultipleProcessingGroups_Test()
        {
            Helper_CreateTestFSM("TestFSM", "Update");
            Helper_CreateTestFSM("TestFSM", "FixedUpdate");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "Update");
            var handle2 = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "FixedUpdate");
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFsmHandleCountInGroup("Update"), Is.EqualTo(1));
            Assert.That(handle2, Is.Not.Null);
            Assert.That(handle2.Context, Is.Not.Null);
            Assert.That(FSM_API.Internal.GetFsmHandleCountInGroup("FixedUpdate"), Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CountIsZeroForNonExistentGroup_Test()
        {
            Assert.That(FSM_API.Internal.GetFsmHandleCountInGroup("NonExistentGroup"), Is.EqualTo(0));
        }


        private void Helper_CreateTestFSM(string fsmName, string processingGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}
