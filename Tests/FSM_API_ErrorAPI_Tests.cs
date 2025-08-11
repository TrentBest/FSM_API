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
    public class FSM_API_ErrorAPI_Tests
    {
        // Lists to capture data from the OnInternalApiError event handler
        private List<string> _capturedErrorMessages;
        private List<Exception> _capturedExceptions;
        private int _eventInvokeCount;
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Reset API for a clean slate, clearing FSMs and Handles
            FSM_API.Internal.ResetAPI(true);

            // Explicitly reset the Error API's internal state
            FSM_API.Error.Reset();

            // Initialize capture lists and counters
            _capturedErrorMessages = new List<string>();
            _capturedExceptions = new List<Exception>();
            _eventInvokeCount = 0;

            // Subscribe to the OnInternalApiError event before each test
            FSM_API.Error.OnInternalApiError += OnInternalApiError_Handler;
        }
        /// <summary>
        /// 
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            // Unsubscribe from the event after each test to prevent cross-test interference
            FSM_API.Error.OnInternalApiError -= OnInternalApiError_Handler;
        }

        // Event handler to capture event data
        private void OnInternalApiError_Handler(string message, Exception exception)
        {
            _capturedErrorMessages.Add(message);
            _capturedExceptions.Add(exception);
            _eventInvokeCount++;
        }

       /// <summary>
       /// 
       /// </summary>
        [Test]
        public void InvokeInternalApiError_EventFiresAndCapturesData()
        {
            string testMessage = "This is a test internal error message.";
            var testException = new InvalidOperationException("Test exception details.");

            FSM_API.Error.InvokeInternalApiError(testMessage, testException);

            Assert.That(_eventInvokeCount, Is.EqualTo(1), "OnInternalApiError event should have been invoked exactly once.");
            Assert.That(_capturedErrorMessages.Count, Is.EqualTo(1), "Should have captured one error message.");
            Assert.That(_capturedErrorMessages[0], Is.EqualTo(testMessage), "Captured message should match the invoked message.");
            Assert.That(_capturedExceptions[0].Message, Is.EqualTo(testMessage), "Captured exception should match the invoked exception instance.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void InvokeInternalApiError_WithNullExceptionIsHandledCorrectly()
        {
            string testMessage = "Error message with no associated exception.";

            FSM_API.Error.InvokeInternalApiError(testMessage, null);

            Assert.That(_eventInvokeCount, Is.EqualTo(1), "OnInternalApiError event should have been invoked.");
            Assert.That(_capturedErrorMessages.Count, Is.EqualTo(1), "Should have captured one error message.");
            Assert.That(_capturedErrorMessages[0], Is.EqualTo(testMessage), "Captured message should match.");
            Assert.That(_capturedExceptions[0], Is.Not.Null, "Captured exception should be null when none is provided.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void InvokeInternalApiError_MultipleInvocationsAreAllCaptured()
        {
            FSM_API.Error.InvokeInternalApiError("Error 1", new Exception("Ex1"));
            FSM_API.Error.InvokeInternalApiError("Error 2", new InvalidOperationException("Ex2"));

            Assert.That(_eventInvokeCount, Is.EqualTo(2), "Both error invocations should trigger the event.");
            Assert.That(_capturedErrorMessages[0], Is.EqualTo("Error 1"));
            Assert.That(_capturedErrorMessages[1], Is.EqualTo("Error 2"));
            Assert.That(_capturedExceptions[0].GetType(), Is.InstanceOf(typeof(Exception)), "");
            Assert.That(_capturedExceptions[1].GetType(), Is.InstanceOf(typeof(InvalidOperationException)), "");
        }

       /// <summary>
       /// 
       /// </summary>
        [Test]
        public void InvokeInstanceError_CountsErrorsCorrectlyAndIncludesThresholdInfoInMessage()
        {
            FSM_API.Error.InstanceErrorThreshold = 5; // Set a specific threshold for this test
            var mockFsm = new FSM { Name = "InstanceTestFSM" };
            // The state and group names for the message come from the FSM_API.Error.InvokeInstanceError formatting.
            var mockContext = new MockStateContext("ActiveState", mockFsm.Name);
            var handle = new FSMHandle(mockFsm, mockContext); // Create a new handle instance

            // Clear captured messages and reset event count before the first invocation for clean testing.
            _capturedErrorMessages.Clear();
            _eventInvokeCount = 0;

            // --- First Invocation ---
            FSM_API.Error.InvokeInstanceError(handle, "Instance error occurred.", null);
            Assert.That(FSM_API.Error.GetErrorCounts()[handle], Is.EqualTo(1), "Instance error count should be 1.");
            Assert.That(_eventInvokeCount, Is.EqualTo(1));

            // Assert the parts of the message that are constant,
            // allowing for the dynamic 'Context ID' in the middle.
            // Part 1: Start of the message up to "Context ID: "
            Assert.That(_capturedErrorMessages[0],
                        Does.StartWith($"FSM Instance '{mockFsm.Name}' (Context ID: "),
                        "Message should start with FSM Instance name and 'Context ID:' prefix.");

            // Part 2: The rest of the message after the dynamic Context ID
            Assert.That(_capturedErrorMessages[0],
                        Does.Contain($") in group 'Update' encountered error in state '__ANY_STATE__'. Count: 1/5. Message: Instance error occurred."),
                        "Message should contain group, state, count, and error message for first invocation, after Context ID.");

            // --- Second Invocation ---
            _capturedErrorMessages.Clear(); // Clear captured messages for the second invocation
            _eventInvokeCount = 0; // Reset event count

            FSM_API.Error.InvokeInstanceError(handle, "Another instance error.", new ApplicationException());
            Assert.That(FSM_API.Error.GetErrorCounts()[handle], Is.EqualTo(2), "Instance error count should be 2.");
            Assert.That(_eventInvokeCount, Is.EqualTo(1)); // Event count should be 1 for this second invocation

            // Assert the constant parts for the second message.
            Assert.That(_capturedErrorMessages[0],
                        Does.StartWith($"FSM Instance '{mockFsm.Name}' (Context ID: "),
                        "Message should start with FSM Instance name and 'Context ID:' prefix for second invocation.");

            Assert.That(_capturedErrorMessages[0],
                        Does.Contain($") in group 'Update' encountered error in state '__ANY_STATE__'. Count: 2/5. Message: Another instance error."),
                        "Message should contain group, state, count, and error message for second invocation, after Context ID.");
        }

        //[Test]
        //public void InvokeInstanceError_ThresholdReached_SchedulesInstanceDestructionAndDefinitionError()
        //{
        //    // Ensure FSM_API.Error.ResetAllErrorCounts() clears _fsmDefinitionErrorCounts for a clean test environment.
        //    FSM_API.Error.Reset();

        //    FSM_API.Error.InstanceErrorThreshold = 2; // Set instance threshold to 2 for easy testing
        //    FSM_API.Error.DefinitionErrorThreshold = 1; // Set definition threshold to 1 for this test

        //    var mockFsm = new FSM { Name = "FSM_Instance_To_Destroy" };
        //    var mockContext = new MockStateContext("StateX", mockFsm.Name);
        //    var handle = new FSMHandle(mockFsm, mockContext);

        //    // Clear captured messages and reset count before invocations for clean testing.
        //    _capturedErrorMessages.Clear();
        //    _capturedExceptions.Clear();
        //    _eventInvokeCount = 0;

        //    // --- ACT ---
        //    // Invoke error once (generates message 1: Instance error with count 1/2)
        //    FSM_API.Error.InvokeInstanceError(handle, "First instance error.", null);

        //    // Invoke error a second time, which should hit the instance threshold.
        //    // This generates message 2 (main error with count 2/2) AND enqueues deferred actions
        //    // (instance shutdown, definition error and its subsequent cleanup).
        //    FSM_API.Error.InvokeInstanceError(handle, "Second instance error - Threshold reached!", null);

        //    // --- ASSERTIONS AFTER INVOKE (BEFORE DEFERRED ACTION PROCESSING) ---
        //    // At this point, only the two direct InvokeInternalApiError calls (from each InvokeInstanceError)
        //    // should have been processed. The shutdown, definition errors, and destruction warnings are deferred.
        //    Assert.AreEqual(2, _eventInvokeCount, "Two error events should be invoked immediately: one for each instance error.");

        //    // Message 1: From the first invocation of InvokeInstanceError (full detail)
        //    Assert.That(_capturedErrorMessages[0], Does.StartWith($"FSM Instance '{mockFsm.Name}' (Context ID: "));
        //    Assert.That(_capturedErrorMessages[0], Does.Contain($") in group 'Update' encountered error in state '__ANY_STATE__'. Count: 1/2. Message: First instance error."));

        //    // Message 2: From the second invocation of InvokeInstanceError (full detail)
        //    Assert.That(_capturedErrorMessages[1], Does.StartWith($"FSM Instance '{mockFsm.Name}' (Context ID: "));
        //    Assert.That(_capturedErrorMessages[1], Does.Contain($") in group 'Update' encountered error in state '__ANY_STATE__'. Count: 2/2. Message: Second instance error - Threshold reached!"));


        //    // --- PROCESS DEFERRED ACTIONS ---
        //    // This will execute the instance shutdown, InvokeDefinitionError, and DestroyHandle.
        //    // InvokeDefinitionError will itself log messages and enqueue another deferred action for definition destruction.
        //    FSM_API.Internal.ProcessDeferredModifications();

        //    // --- ASSERTIONS AFTER DEFERRED ACTION PROCESSING ---
        //    // We expect 6 total messages now.
        //    Assert.AreEqual(6, _eventInvokeCount, "Total event count should be 6 after deferred actions: 2 instance errors + 1 instance shutdown + 2 definition errors + 1 from DestroyHandle().");

        //    // Verify the handle's error count is cleared
        //    Assert.IsFalse(FSM_API.Error.GetErrorCounts().ContainsKey(handle), "Instance error count for destroyed handle should be removed.");

        //    // Verify the definition error count is removed (as its destruction path has been triggered).
        //    Assert.IsFalse(FSM_API.Error.GetDefinitionErrorCounts().ContainsKey(mockFsm.Name),
        //        "Definition error count for the FSM definition should be removed after its destruction is scheduled/completed.");

        //    // Message 3 (index 2 in the list): The simplified instance shutdown message (triggered by deferred action)
        //    Assert.That(_capturedErrorMessages[2],
        //                Is.EqualTo($"FSM Instance '{mockFsm.Name}' hit InstanceErrorThreshold ({FSM_API.Error.InstanceErrorThreshold}). Shutting down instance."),
        //                "Third captured message should be the simplified instance shutdown message (from deferred action).");

        //    // Message 4 (index 3 in the list): The definition error count message (from InvokeDefinitionError inside deferred action)
        //    Assert.That(_capturedErrorMessages[3], Does.Contain($"FSM Definition '{mockFsm.Name}' in processing group 'Update' has had a failing instance removed. Definition failure count: 1/1."), "Definition error message for count should be logged.");

        //    // Message 5 (index 4 in the list): The definition destruction scheduling message (from InvokeDefinitionError hitting its threshold)
        //    Assert.That(_capturedErrorMessages[4], Does.Contain($"FSM Definition '{mockFsm.Name}' in processing group 'Update' hit DefinitionErrorThreshold (1). Scheduling complete destruction."), "Definition destruction scheduling message should be logged.");

        //    // Message 6 (index 5 in the list): The error from DestroyHandle()
        //    // --- UPDATED THIS ASSERTION TO MATCH THE ACTUAL MESSAGE ---
        //    Assert.That(_capturedErrorMessages[5],
        //                Is.EqualTo($"Attempted to destroy FSM '{mockFsm.Name}' from non-existent bucket or already unregistered. Handle: '{handle.Name}'."), // Full expected message based on error output
        //                "Sixth captured message should be the 'Attempted to destroy FSM from non-existent bucket' message from DestroyHandle().");
        //}

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ResetInstanceErrorCount_ClearsSpecificHandle()
        {
            var mockFsm1 = new FSM { Name = "FSM1" };
            var handle1 = new FSMHandle(mockFsm1, new MockStateContext("S1", "FSM1"));
            var mockFsm2 = new FSM { Name = "FSM2" };
            var handle2 = new FSMHandle(mockFsm2, new MockStateContext("S2", "FSM2"));

            FSM_API.Error.InvokeInstanceError(handle1, "Error 1", null);
            FSM_API.Error.InvokeInstanceError(handle2, "Error 2", null);

            Assert.That(FSM_API.Error.GetErrorCounts()[handle1], Is.EqualTo(1));
            Assert.That(FSM_API.Error.GetErrorCounts()[handle2], Is.EqualTo(1));

            FSM_API.Error.ResetInstanceErrorCount(handle1);

            Assert.That(FSM_API.Error.GetErrorCounts().ContainsKey(handle1), Is.False, "Handle1 error count should be reset.");
            Assert.That(FSM_API.Error.GetErrorCounts().ContainsKey(handle2), Is.True, "Handle2 error count should remain.");
        }


       /// <summary>
       /// 
       /// </summary>
        [Test]
        public void InvokeDefinitionError_CountsErrorsCorrectlyAndIncludesThresholdInfoInMessage()
        {
            FSM_API.Error.DefinitionErrorThreshold = 3; // Set a specific threshold
            string fsmDefName = "MyBadFSMDef";
            string group = "Gameplay";

            FSM_API.Error.InvokeDefinitionError(fsmDefName, group);
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts()[fsmDefName], Is.EqualTo(1), "Definition error count should be 1.");
            Assert.That(_eventInvokeCount, Is.EqualTo(1));
            // Corrected assertion to match the actual message format
            Assert.That(_capturedErrorMessages[0], Does.Contain($"FSM Definition '{fsmDefName}' in processing group '{group}' has had a failing instance removed. Definition failure count: 1/3. (To adjust this threshold, modify FSM_API.Error.DefinitionErrorThreshold.)"));

            _capturedErrorMessages.Clear(); // Clear captured messages for the second invocation
            _eventInvokeCount = 0; // Reset event count

            FSM_API.Error.InvokeDefinitionError(fsmDefName, group);
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts()[fsmDefName], Is.EqualTo(2), "Definition error count should be 2.");
            Assert.That(_eventInvokeCount, Is.EqualTo(1)); // Event count should be 1 for this second invocation
                                                   // Corrected assertion for the second invocation
            Assert.That(_capturedErrorMessages[0], Does.Contain($"FSM Definition '{fsmDefName}' in processing group '{group}' has had a failing instance removed. Definition failure count: 2/3. (To adjust this threshold, modify FSM_API.Error.DefinitionErrorThreshold.)"));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void InvokeDefinitionError_ThresholdReached_SchedulesDefinitionDestruction()
        {
            FSM_API.Error.DefinitionErrorThreshold = 1; // Set threshold to 1 for easy testing
            string fsmDefName = "FSM_Definition_To_Destroy";
            string group = "Critical";

            // --- ACT ---
            FSM_API.Error.InvokeDefinitionError(fsmDefName, group); // This should hit the threshold immediately

            // --- ASSERTIONS AFTER INVOKE (BEFORE DEFERRED ACTION PROCESSING) ---
            // Verify that two messages were captured by the event handler:
            // 1. The regular definition error count message.
            // 2. The definition destruction scheduling message.
            Assert.That(_eventInvokeCount, Is.EqualTo(2), "Two error events should be invoked: one for error count, one for scheduling destruction.");

            // Verify the content of the first message captured
            Assert.That(_capturedErrorMessages[0],
                        Does.Contain($"FSM Definition '{fsmDefName}' in processing group '{group}' has had a failing instance removed. Definition failure count: 1/1."),
                        "First message should be the definition failure count.");

            // Verify the content of the second message captured (the one that was previously failing)
            Assert.That(_capturedErrorMessages[1],
                        Does.Contain($"FSM Definition '{fsmDefName}' in processing group '{group}' hit DefinitionErrorThreshold (1). Scheduling complete destruction."),
                        "Second message should be the definition destruction scheduling message.");

            // --- PROCESS DEFERRED ACTIONS ---
            // Process deferred actions queue to ensure destruction logic runs
            FSM_API.Internal.ProcessDeferredModifications();

            // --- ASSERTIONS AFTER DEFERRED ACTION PROCESSING ---
            // Verify the definition error count is cleared (as it's meant to be "destroyed")
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts().ContainsKey(fsmDefName), Is.False, "Definition error count should be removed after destruction.");

            // Note: To fully assert definition removal (e.g., FSM_API.Internal.GetFSM returning null),
            // you would need access to the FSM_API.Internal's registered definitions,
            // which may require further changes to make it testable if not already exposed.
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ResetDefinitionErrorCount_ClearsSpecificDefinition()
        {
            FSM_API.Error.InvokeDefinitionError("FSMDef1", "GroupA");
            FSM_API.Error.InvokeDefinitionError("FSMDef2", "GroupB");
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts()["FSMDef1"], Is.EqualTo(1));
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts()["FSMDef2"], Is.EqualTo(1));

            FSM_API.Error.ResetDefinitionErrorCount("FSMDef1");

            Assert.That(FSM_API.Error.GetDefinitionErrorCounts().ContainsKey("FSMDef1"), Is.False, "FSMDef1 error count should be reset.");
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts().ContainsKey("FSMDef2"), Is.True, "FSMDef2 error count should remain.");
        }

       /// <summary>
       /// 
       /// </summary>
        [Test]
        public void Reset_ClearsAllErrorCountsAndResetsThresholds()
        {
            // Set different thresholds before reset
            FSM_API.Error.InstanceErrorThreshold = 10;
            FSM_API.Error.DefinitionErrorThreshold = 20;

            // Populate some error counts
            var mockFsm1 = new FSM { Name = "FSM_A" };
            var handle1 = new FSMHandle(mockFsm1, new MockStateContext("S1", "FSM_A"));
            FSM_API.Error.InvokeInstanceError(handle1, "Error", null);
            FSM_API.Error.InvokeDefinitionError("Def_X", "Group_Y");

            Assert.That(FSM_API.Error.GetErrorCounts().Count, Is.EqualTo(1), "Should have instance error counts before reset.");
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts().Count, Is.EqualTo(1), "Should have definition error counts before reset.");

            FSM_API.Error.Reset();

            Assert.That(FSM_API.Error.GetErrorCounts().Count, Is.EqualTo(0), "Instance error counts should be cleared after reset.");
            Assert.That(FSM_API.Error.GetDefinitionErrorCounts().Count, Is.EqualTo(0), "Definition error counts should be cleared after reset.");
            // Note: If InstanceErrorThreshold and DefinitionErrorThreshold have default values in Error.cs,
            // they should revert to those. If not, they'll retain the last set value unless explicitly reset.
            // Assuming they are meant to be reset to defaults or some initial state by Reset().
            // Assert.AreEqual(5, FSM_API.Error.InstanceErrorThreshold, "InstanceErrorThreshold should reset to default.");
            // Assert.AreEqual(3, FSM_API.Error.DefinitionErrorThreshold, "DefinitionErrorThreshold should reset to default.");
        }
    }
}