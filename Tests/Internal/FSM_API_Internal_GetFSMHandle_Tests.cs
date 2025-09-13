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
    public class FSM_API_Internal_GetFSMHandle_Tests
    {
        private const string FsmName = "TestFSM";
        private const string ProcessGroup = "Update";
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
            FSM_API.Create.CreateFiniteStateMachine(FsmName, processingGroup: ProcessGroup).BuildDefinition();
        }

        /// <summary>
        /// Tests that GetFSMHandle returns the correct handle when it exists.
        /// </summary>
        [Test]
        public void GetFSMHandle_SucceedsWithExistingHandle()
        {
            // Arrange
            var context = new FSMTestContext();
            var handle = FSM_API.Create.CreateInstance(FsmName, context, ProcessGroup);

            // Act
            var retrievedHandle = FSM_API.Internal.GetFSMHandle(FsmName, context, ProcessGroup);

            // Assert
            Assert.That(retrievedHandle, Is.Not.Null);
            Assert.That(retrievedHandle, Is.EqualTo(handle));
        }

        /// <summary>
        /// Tests that GetFSMHandle returns null for a non-existent FSM name.
        /// </summary>
        [Test]
        public void GetFSMHandle_ReturnsNullForNonExistentFsm()
        {
            // Arrange
            var context = new FSMTestContext();
            FSM_API.Create.CreateInstance(FsmName, context, ProcessGroup);

            // Act
            var retrievedHandle = FSM_API.Internal.GetFSMHandle("NonExistentFSM", context, ProcessGroup);

            // Assert
            Assert.That(retrievedHandle, Is.Null);
        }

        /// <summary>
        /// Tests that GetFSMHandle returns null for a non-existent processing group.
        /// </summary>
        [Test]
        public void GetFSMHandle_ReturnsNullForNonExistentProcessingGroup()
        {
            // Arrange
            var context = new FSMTestContext();
            FSM_API.Create.CreateInstance(FsmName, context, ProcessGroup);

            // Act
            var retrievedHandle = FSM_API.Internal.GetFSMHandle(FsmName, context, "NonExistentGroup");

            // Assert
            Assert.That(retrievedHandle, Is.Null);
        }

        /// <summary>
        /// Tests that GetFSMHandle returns null for a non-existent context.
        /// </summary>
        [Test]
        public void GetFSMHandle_ReturnsNullForNonExistentContext()
        {
            // Arrange
            FSM_API.Create.CreateInstance(FsmName, new FSMTestContext(), ProcessGroup);

            // Act
            var retrievedHandle = FSM_API.Internal.GetFSMHandle(FsmName, new FSMTestContext(), ProcessGroup);

            // Assert
            Assert.That(retrievedHandle, Is.Null);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFSMHandle_Test()
        {
            FSMTestContext context = new FSMTestContext();
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", -1, "Update").BuildDefinition();
            FSM_API.Create.CreateInstance("TestFSM", context, "Update");
            var handle = FSM_API.Internal.GetFSMHandle("TestFSM", context, "Update");
            Assert.That(handle, Is.Not.Null, "GetFSMHandle should return null when no FSM is defined.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFSMHandle_InvalidFSM_Test()
        {
            FSMTestContext context = new FSMTestContext();
            var handle = FSM_API.Internal.GetFSMHandle("InvalidFSM", context, "Update");
            Assert.That(handle, Is.Null, "GetFSMHandle should return null when the FSM is not defined.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFSMHandle_InvalidContext_Test()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", -1, "Update").BuildDefinition();
            var handle = FSM_API.Internal.GetFSMHandle("TestFSM", null, "Update");
            Assert.That(handle, Is.Null, "GetFSMHandle should return null when the context is null.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFSMHandle_InvalidProcessingGroup_Test()
        {
            FSMTestContext context = new FSMTestContext();
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", -1, "Update").BuildDefinition();
            var handle = FSM_API.Internal.GetFSMHandle("TestFSM", context, "InvalidGroup");
            Assert.That(handle, Is.Null, "GetFSMHandle should return null when the processing group is invalid.");
        }
    }
}
