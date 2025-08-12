using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API.Tests.Internal;


namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// Tests for the FSMHandle class.
    /// </summary>
    [TestFixture]
    public class FSM_API_FSMHandle_Tests
    {
        
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Reset the API for each test to ensure a clean slate
            FSM_API.Internal.ResetAPI(true);

        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_IsCreatedWithValidName()
        {
          Helper_CreateFSM("TestFSM", "TestGroup");
            // Act
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            // Assert
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Name, Is.EqualTo("TestFSM"));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandlesNotEqual()
        {
            Helper_CreateFSM("TestFSM", "TestGroup");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            var anotherHandle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            // Assert that two different instances of FSMHandle are not equal
            Assert.That(handle, Is.Not.EqualTo(anotherHandle));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_EqualityOperator()
        {
            Helper_CreateFSM("TestFSM", "TestGroup");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            var anotherHandle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            // Assert that the equality operator works as expected
            Assert.That(handle == anotherHandle, Is.False);
            Assert.That(handle != anotherHandle, Is.True);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_EqualityOperator_WithSameInstance()
        {
            Helper_CreateFSM("TestFSM", "TestGroup");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            // Assert that the equality operator works as expected for the same instance
            Assert.That(handle == handle, Is.True);
            Assert.That(handle != handle, Is.False);
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_EqualityOperator_WithNull()
        {
            Helper_CreateFSM("TestFSM", "TestGroup");
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");
            // Assert that the equality operator works as expected with null
            Assert.That(handle == null, Is.False);
            Assert.That(handle != null, Is.True);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_EqualityOperator_WithNullInstance()
        {
            Helper_CreateFSM("TestFSM", "TestGroup");
            FSMHandle handle = null;
            // Assert that the equality operator works as expected with a null instance
            Assert.That(handle == null, Is.True);
            Assert.That(handle != null, Is.False);
        }

        private void Helper_CreateFSM(string fsmName, string processingGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}