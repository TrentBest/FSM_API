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
            Assert.That(handle , Is.EqualTo(handle));
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
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_Constructor_SucceedsAndSetsPropertiesCorrectly()
        {
            // Arrange
            var fsmDefinition = new FSM { Name = "TestFSM" };
            var context = new MockStateContext("InitialState", "TestFSM");
            var id = 1;

            // Act
            var handle = new FSMHandle(fsmDefinition, context, id);

            // Assert
            Assert.That(handle, Is.Not.Null);
            Assert.That(handle.Definition, Is.EqualTo(fsmDefinition));
            Assert.That(handle.Context, Is.EqualTo(context));
            Assert.That(handle.Id, Is.EqualTo(id));
            Assert.That(handle.CurrentState, Is.EqualTo("__ANY_STATE__"));
            Assert.That(handle.Name, Is.EqualTo("TestFSM"));
            Assert.That(handle.HasEnteredCurrentState, Is.False);
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_Context_PropertyIsAccessible()
        {
            // Arrange
            var fsmDefinition = new FSM { Name = "TestFSM" };
            var context = new MockStateContext("InitialState", "TestFSM");
            var handle = new FSMHandle(fsmDefinition, context, 1);

            // Act
            var retrievedContext = handle.Context;

            // Assert
            Assert.That(retrievedContext, Is.EqualTo(context));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_Name_PropertyIsAccessible()
        {
            // Arrange
            var fsmDefinition = new FSM { Name = "TestFSM" };
            var context = new MockStateContext("InitialState", "TestFSM");
            var handle = new FSMHandle(fsmDefinition, context, 1);

            // Act
            var retrievedName = handle.Name;

            // Assert
            Assert.That(retrievedName, Is.EqualTo(fsmDefinition.Name));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMHandle_ID_PropertyIsAccessible()
        {
            // Arrange
            var fsmDefinition = new FSM { Name = "TestFSM" };
            var context = new MockStateContext("InitialState", "TestFSM");
            var id = 123;
            var handle = new FSMHandle(fsmDefinition, context, id);

            // Act
            var retrievedId = handle.Id;

            // Assert
            Assert.That(retrievedId, Is.EqualTo(id));
        }

        private void Helper_CreateFSM(string fsmName, string processingGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}