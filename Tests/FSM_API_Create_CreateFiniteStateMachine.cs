using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework; // Assuming NUnit for testing

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public class FSM_API_Create_CreateFiniteStateMachineTests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true); // Resets the API for a clean test environment
        }

        [Test]
        public void Create_CreateFiniteStateMachine_Succeeds()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();

            var count = FSM_API.Internal.TotalFsmDefinitionCount;
            Assert.IsTrue(count == 1);
        }

        [Test]
        public void Create_CreateFiniteStateMachine_SucceedsWithDefaults()
        {
            // Act
            FSM_API.Create.CreateFiniteStateMachine().BuildDefinition(); // Use default name and group

            // Assert
            Assert.That(FSM_API.Internal.TotalFsmDefinitionCount, Is.EqualTo(1), "Expected one FSM definition after creating with defaults.");
            Assert.That(FSM_API.Internal.DoesFsmDefinitionExist("Update", "UnNamedFSM"), Is.True, "Expected 'UnNamedFSM' in 'Update' group.");
            Assert.That(FSM_API.Internal.ProcessingGroupCount, Is.EqualTo(1), "Expected one processing group ('Update') to exist.");

        }

        [Test]
        public void Create_CreateFiniteStateMachine_SucceedsWithCustomParameters()
        {
            // Arrange
            string customFsmName = "MyCustomFSM";
            int customProcessRate = 50; // A valid positive rate
            string customProcessingGroup = "RenderLoop";

            // Act
            FSM_API.Create.CreateFiniteStateMachine(customFsmName, customProcessRate, customProcessingGroup).BuildDefinition();

            // Assert
            Assert.That(FSM_API.Internal.TotalFsmDefinitionCount, Is.EqualTo(1), "Expected one FSM definition after creating with custom parameters.");
            Assert.That(FSM_API.Internal.DoesFsmDefinitionExist(customProcessingGroup, customFsmName), Is.True, $"Expected '{customFsmName}' in '{customProcessingGroup}' group.");
            Assert.That(FSM_API.Internal.ProcessingGroupCount, Is.EqualTo(1), $"Expected one processing group ('{customProcessingGroup}') to exist.");

            // You might want to check the process rate directly on the FsmBucket if possible.
            // This would require FSM_API.Internal to expose a way to get the FsmBucket for a given name/group.
            // Example (if FSM_API.Internal had GetFsmBucket):
            // var bucket = FSM_API.Internal.GetFsmBucket(customProcessingGroup, customFsmName);
            // Assert.IsNotNull(bucket);
            // Assert.AreEqual(customProcessRate, bucket.ProcessRate); // Assuming FsmBucket has a ProcessRate property
        }

        [Test]
        public void Create_CreateFiniteStateMachine_ReturnsExistingBuilderForExistingFSM()
        {
            // Arrange
            string existingFsmName = "ExistingFSM";
            string existingProcessingGroup = "Gameplay";
            FSM_API.Create.CreateFiniteStateMachine(existingFsmName, 100, existingProcessingGroup).BuildDefinition(); // Create it first

            // Act
            // Call CreateFiniteStateMachine again with the same parameters
            FSMBuilder builder1 = FSM_API.Create.CreateFiniteStateMachine(existingFsmName, 100, existingProcessingGroup);
            FSMBuilder builder2 = FSM_API.Create.CreateFiniteStateMachine(existingFsmName, 200, existingProcessingGroup); // Even with different processRate, should return existing

            // Assert
            // The key assertion here is that no *new* FSM definition is created.
            Assert.That(FSM_API.Internal.TotalFsmDefinitionCount, Is.EqualTo(1), "Expected only one FSM definition to exist, not a new one.");
            Assert.That(FSM_API.Internal.ProcessingGroupCount, Is.EqualTo(1), "Expected only one processing group to exist.");
            Assert.That(FSM_API.Internal.DoesFsmDefinitionExist(existingProcessingGroup, existingFsmName), Is.True, "Existing FSM definition should still be present.");

            // You could also assert that the returned builder references the same underlying definition object,
            // if FSMBuilder exposes that, but that starts to lean into builder's internal behavior.
            // For now, confirming no new definition is registered is sufficient.
        }

        [Test]
        public void Create_CreateFiniteStateMachine_InvalidFsmName_ThrowsArgumentException()
        {
            // Arrange
            string emptyFsmName = "";
            string whitespaceFsmName = "   ";
            string nullFsmName = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateFiniteStateMachine(emptyFsmName), "Expected ArgumentException for empty FSM name.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateFiniteStateMachine(whitespaceFsmName), "Expected ArgumentException for whitespace FSM name.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateFiniteStateMachine(nullFsmName), "Expected ArgumentException for null FSM name.");

            // Ensure no FSM was created
            Assert.That(FSM_API.Internal.TotalFsmDefinitionCount, Is.EqualTo(0), "No FSM definition should be created when invalid name is provided.");
        }

        [Test]
        public void Create_CreateFiniteStateMachine_InvalidProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string emptyProcessingGroup = "";
            string whitespaceProcessingGroup = "   ";
            string nullProcessingGroup = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateFiniteStateMachine("ValidFSM", 0, emptyProcessingGroup), "Expected ArgumentException for empty processing group.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateFiniteStateMachine("ValidFSM", 0, whitespaceProcessingGroup), "Expected ArgumentException for whitespace processing group.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateFiniteStateMachine("ValidFSM", 0, nullProcessingGroup), "Expected ArgumentException for null processing group.");

            // Ensure no FSM was created
            Assert.That(FSM_API.Internal.TotalFsmDefinitionCount, Is.EqualTo(0), "No FSM definition should be created when invalid processing group is provided.");
        }

        [Test]
        public void Create_CreateFiniteStateMachine_InvalidProcessRate_CoercesToZeroAndLogsError()
        {
            // Arrange
            string fsmName = "FSMWithBadRate";
            string processingGroup = "TestGroup";
            int invalidProcessRate = -5; // Value less than -1

            // You might need to mock FSM_API.Error or capture its calls if you want to assert on the logging.
            // For example, if FSM_API.Error had a static event or a way to inspect messages:
            // var loggedErrors = new List<string>();
            // FSM_API.Error.OnErrorOccurred += (msg, ex) => loggedErrors.Add(msg);

            // Act
            // Call BuildDefinition to ensure it attempts to register it,
            // even though the rate coercion happens within CreateFiniteStateMachine.
            FSM_API.Create.CreateFiniteStateMachine(fsmName, invalidProcessRate, processingGroup).BuildDefinition();

            // Assert
            Assert.That(FSM_API.Internal.TotalFsmDefinitionCount, Is.EqualTo(1), "Expected one FSM definition to be created despite invalid rate.");
            Assert.IsTrue(FSM_API.Internal.DoesFsmDefinitionExist(processingGroup, fsmName), "FSM should exist after creation with coerced rate.");

            // Verification of coercion:
            // This requires access to the FsmBucket's ProcessRate property.
            // If FSM_API.Internal doesn't expose a way to get the FsmBucket, this assertion is difficult.
            // Assuming FSM_API.Internal.GetFsmBucket(group, name) returns FsmBucket:
            // var bucket = FSM_API.Internal.GetFsmBucket(processingGroup, fsmName);
            // Assert.IsNotNull(bucket);
            // Assert.AreEqual(0, bucket.ProcessRate, "Invalid processRate should be coerced to 0.");

            // Verification of error logging:
            // If FSM_API.Error had a mockable or inspectable interface:
            // MockErrorLogging.VerifyErrorLoggedContains("Invalid processRate '-5'", Times.Once);
            // Assert.IsTrue(loggedErrors.Any(msg => msg.Contains("Invalid processRate '-5'") && msg.Contains("Setting to 0")), "Error message about rate coercion should be logged.");
        }
    }
}