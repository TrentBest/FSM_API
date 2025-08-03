using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static TheSingularityWorkshop.FSM_API.FSM_API.Internal;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public class FSM_API_Create_CreateInstanceTests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void CreateInstance_SucceedsWithDefaultProcessingGroup()
        {
            // Arrange
            string fsmName = "PlayerFSM";
            IStateContext context = new FSMTestContext();
            // Define the FSM first, as CreateInstance requires an existing definition
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();

            // Act
            FSMHandle handle = FSM_API.Create.CreateInstance(fsmName, context);

            // Assert
            Assert.IsNotNull(handle, "FSMHandle should not be null.");
            Assert.That(handle.Context, Is.EqualTo(context), "FSMHandle context should match the provided context.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1), "Expected one FSM instance to be created.");
            // You could also assert on the FSM definition associated with the handle if FSMHandle exposes it
            // Assert.AreEqual(fsmName, handle.Definition.Name);
        }

        [Test]
        public void CreateInstance_SucceedsWithCustomProcessingGroup()
        {
            // Arrange
            string fsmName = "EnemyAI";
            string customProcessingGroup = "FixedUpdate";
            IStateContext context = new FSMTestContext();
            // Define the FSM in the custom processing group
            FSM_API.Create.CreateFiniteStateMachine(fsmName, processingGroup: customProcessingGroup).BuildDefinition();

            // Act
            FSMHandle handle = FSM_API.Create.CreateInstance(fsmName, context, customProcessingGroup);

            // Assert
            Assert.IsNotNull(handle, "FSMHandle should not be null.");
            Assert.That(handle.Context, Is.EqualTo(context), "FSMHandle context should match the provided context.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1), "Expected one FSM instance to be created.");
        }

        [Test]
        public void CreateInstance_MultipleInstancesForSameFSM_Succeeds()
        {
            // Arrange
            string fsmName = "DoorFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();

            IStateContext context1 = new FSMTestContext();
            IStateContext context2 = new FSMTestContext();
            IStateContext context3 = new FSMTestContext();

            // Act
            FSMHandle handle1 = FSM_API.Create.CreateInstance(fsmName, context1);
            FSMHandle handle2 = FSM_API.Create.CreateInstance(fsmName, context2);
            FSMHandle handle3 = FSM_API.Create.CreateInstance(fsmName, context3);

            // Assert
            Assert.IsNotNull(handle1);
            Assert.IsNotNull(handle2);
            Assert.IsNotNull(handle3);
            Assert.That(handle2, Is.Not.SameAs(handle1), "Handles should be distinct instances.");
            Assert.That(handle3, Is.Not.SameAs(handle1), "Handles should be distinct instances.");
            Assert.That(handle3, Is.Not.SameAs(handle2), "Handles should be distinct instances.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(3), "Expected three FSM instances to be created.");
        }

        [Test]
        public void CreateInstance_InvalidFsmName_ThrowsArgumentException()
        {
            // Arrange
            IStateContext context = new FSMTestContext();
            string validFsmName = "ValidFSM";
            FSM_API.Create.CreateFiniteStateMachine(validFsmName).BuildDefinition(); // Ensure one valid FSM exists

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateInstance(string.Empty, context), "Expected ArgumentException for null FSM name.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateInstance("", context), "Expected ArgumentException for empty FSM name.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateInstance("   ", context), "Expected ArgumentException for whitespace FSM name.");

            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0), "No FSM instance should be created with invalid FSM name.");
        }

        [Test]
        public void CreateInstance_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            string fsmName = "SomeFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            FSMTestContext? tc = null;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => FSM_API.Create.CreateInstance(fsmName, tc!), "Expected ArgumentNullException for null context.");

            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0), "No FSM instance should be created with null context.");
        }

        [Test]
        public void CreateInstance_InvalidProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string fsmName = "AnotherFSM";
            IStateContext context = new FSMTestContext();
            FSM_API.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateInstance(fsmName, context, string.Empty), "Expected ArgumentException for null processing group.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateInstance(fsmName, context, ""), "Expected ArgumentException for empty processing group.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateInstance(fsmName, context, "   "), "Expected ArgumentException for whitespace processing group.");

            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0), "No FSM instance should be created with invalid processing group.");
        }

        [Test]
        public void CreateInstance_FSMDefinitionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            string nonExistentFsmName = "NonExistentFSM";
            string existingFsmName = "ExistingFSM";
            string nonExistentGroup = "NonExistentGroup";
            string existingGroup = "Update";
            IStateContext context = new FSMTestContext();

            // Create one FSM to ensure the system is not entirely empty
            FSM_API.Create.CreateFiniteStateMachine(existingFsmName, processingGroup: existingGroup).BuildDefinition();

            // Act & Assert 1: FSM name not found in existing group
            Assert.Throws<KeyNotFoundException>(() => FSM_API.Create.CreateInstance(nonExistentFsmName, context, existingGroup),
                "Expected KeyNotFoundException when FSM name is not found in the specified existing group.");

            // Act & Assert 2: Processing group not found at all
            Assert.Throws<KeyNotFoundException>(() => FSM_API.Create.CreateInstance(existingFsmName, context, nonExistentGroup),
                "Expected KeyNotFoundException when processing group itself is not found.");

            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0), "No FSM instance should be created when definition is not found.");
        }

        [Test]
        public void CreateInstance_BucketDefinitionIsNull_UsesDefaultFSMAndSucceeds()
        {
            // Arrange
            string fsmName = "FSMWithNullDefinition";
            string processingGroup = "SpecialGroup";
            IStateContext context = new FSMTestContext();

            // Simulate a scenario where a bucket exists but its Definition is null.
            // This requires direct manipulation of the internal _buckets, which is generally
            // not recommended for unit tests, but for covering this specific internal branch,
            // it's necessary. This implies FSM_API.Internal needs to expose a way to do this,
            // or we mock FSM_API.Internal.GetBuckets().
            // For simplicity, let's assume FSM_API.Internal.GetBuckets() returns the actual _buckets.
            // And we can manually add an FsmBucket with a null definition.

            // First, ensure the processing group exists
            FSM_API.Create.CreateFiniteStateMachine("DummyFSM", processingGroup: processingGroup).BuildDefinition();
            // Now, directly manipulate the bucket to set its definition to null (simulating a bug/edge case)
            var buckets = FSM_API.Internal.GetBuckets();
            if (buckets.TryGetValue(processingGroup, out var categoryBuckets) && categoryBuckets.TryGetValue("DummyFSM", out var bucketToModify))
            {
                // Remove the dummy FSM and add one with a null definition for our test target
                categoryBuckets.Remove("DummyFSM");
                categoryBuckets.Add(fsmName, new FsmBucket { Definition = null, Instances = new List<FSMHandle>() });
            }
            else
            {
                // This scenario should not happen if DummyFSM was created correctly.
                Assert.Fail("Failed to set up test condition: could not find or modify bucket for FSMWithNullDefinition.");
            }

            // Act
            FSMHandle handle = FSM_API.Create.CreateInstance(fsmName, context, processingGroup);

            // Assert
            Assert.IsNotNull(handle, "FSMHandle should be created even if bucket.Definition was initially null.");
            Assert.IsNotNull(handle.Definition, "FSMHandle should have a non-null definition (the default FSM).");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1), "Expected one FSM instance to be created.");
            // You could further assert that handle.Definition.Name matches the default FSM name if GetDefaultFSM has a known name.
            // Assert.AreEqual(FSM_API.Internal.GetDefaultFSM().Name, handle.Definition.Name);
        }
    }

    
}
