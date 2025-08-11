using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

// Mock implementations for testing purposes
// These would typically be in a separate TestHelpers or Mocks namespace
// but are included here for self-containment of the example.

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public static class Error
    {
        /// <summary>
        /// 
        /// </summary>
        public static List<string> RecordedErrors = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="processGroup"></param>
        public static void InvokeInstanceError(FSMHandle handle, string message, Exception ex, string processGroup = "Update")
        {
            RecordedErrors.Add($"Error: {message} | FSM: {handle?.Name} | Exception: {ex?.Message} | Group: {processGroup}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Internal
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullReset"></param>
        public static void ResetAPI(bool fullReset)
        {
            // In a real scenario, this would clear internal FSM API state.
            // For these tests, we just reset the recorded errors.
            Error.RecordedErrors.Clear();
        }
    }
}

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
            var handle = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "TestGroup");
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
            var handle = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "TestGroup");
            var anotherHandle = FSM_API.Create.CreateInstance("TestFSM", new TestContext(), "TestGroup");
            // Assert that two different instances of FSMHandle are not equal
            Assert.That(handle, Is.Not.EqualTo(anotherHandle));
        }

        private void Helper_CreateFSM(string fsmName, string processingGroup)
        {
            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, processingGroup)
                .State("TestState", null, null, null)
                .BuildDefinition();
        }
    }
}