using NUnit.Framework;

using System;
using System.Diagnostics;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Create_CreateProcessingGroup_Tests
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
        public void CreateProcessingGroup_Succeeds()
        {
            // ARRANGE
            var groupName = "MySuccessfulGroup";

            // ACT
            FSM_API.Create.CreateProcessingGroup(groupName);

            // ASSERT
            var buckets = FSM_API.Internal.GetBuckets();
            Assert.That(buckets, Contains.Key(groupName), "Processing group should be created.");
            Assert.That(buckets[groupName], Is.Not.Null, "Created processing group dictionary should not be null.");
            Assert.That(buckets[groupName], Is.Empty, "Newly created processing group should be empty.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreateProcessingGroup_MaximumLengthName_Succeeds()
        {
            // ARRANGE
            var maxLengthName = new string('A', 255); // Assuming 255 is the max length allowed
            // ACT
            FSM_API.Create.CreateProcessingGroup(maxLengthName);
            // ASSERT
            var buckets = FSM_API.Internal.GetBuckets();
            Assert.That(buckets, Contains.Key(maxLengthName), "Processing group with maximum length name should be created.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test, Explicit("Stress tests take time and memory. Run manually.")]
        public void CreateProcessingGroup_CanHandle_LargeVolume_OfGroups()
        {
            // ARRANGE
            // 100,000 is a heavy load but safe for most CI/CD runners and dev machines.
            // Going higher (e.g. 1 million) might trigger OutOfMemory depending on your RAM.
            const int largeVolume = 100_000;

            // ACT
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < largeVolume; i++)
            {
                // Using a simple index as the name ensures uniqueness
                FSM_API.Create.CreateProcessingGroup($"Group_{i}");
            }

            sw.Stop();
            Console.WriteLine($"Created {largeVolume} groups in {sw.ElapsedMilliseconds}ms");

            // ASSERT
            var buckets = FSM_API.Internal.GetBuckets();

            Assert.That(buckets.Count, Is.EqualTo(largeVolume),
                $"Should have successfully created {largeVolume} distinct processing groups.");

            Assert.That(FSM_API.Internal.ProcessingGroupCount, Is.EqualTo(largeVolume),
                "Internal property ProcessingGroupCount should match the dictionary count.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invalidGroupName"></param>
        [TestCase("")]
        [TestCase("   ")]
        public void CreateProcessingGroup_InvalidNames_ThrowsArgumentException(string invalidGroupName)
        {
            // ACT
            // The CreateProcessingGroup method should internally validate its input.
            // We'll assert that it throws the expected exception.
            var ex = Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateProcessingGroup(invalidGroupName));

            // ASSERT
            Assert.That(ex.Message, Does.Contain("Processing group cannot be null or empty."), "Exception message should indicate invalid group name.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreateProcessingGroup_MaxLengthName_Succeeds()
        {
            // ARRANGE
            // Max string length is platform dependent, but typically 2GB. Let's use a reasonably long string.
            var longGroupName = new string('a', 255);

            // ACT
            FSM_API.Create.CreateProcessingGroup(longGroupName);

            // ASSERT
            var buckets = FSM_API.Internal.GetBuckets();
            Assert.That(buckets, Contains.Key(longGroupName), "Long processing group name should be created.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreateProcessingGroup_ExistingName_ReturnsExistingGroup()
        {
            // ARRANGE
            var groupName = "ExistingGroup";
            FSM_API.Create.CreateProcessingGroup(groupName); // Create it once

            // Add a dummy FSM to ensure the bucket isn't empty on subsequent check
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", processingGroup: groupName).State("TestState",null!, null!,null!).BuildDefinition();
            var initialBucketState = FSM_API.Internal.GetBuckets()[groupName];
            var initialFSMCount = initialBucketState.Count;

            // ACT
            FSM_API.Create.CreateProcessingGroup(groupName); // Call again with the same name

            // ASSERT
            var bucketsAfterSecondCall = FSM_API.Internal.GetBuckets();
            Assert.That(bucketsAfterSecondCall, Contains.Key(groupName), "Processing group should still exist.");

            // Verify it's the same bucket (not recreated) by checking its contents or reference
            // A simple way to check is that the FSM count hasn't changed if it wasn't re-initialized.
            Assert.That(bucketsAfterSecondCall[groupName].Count, Is.EqualTo(initialFSMCount),
                        "Calling CreateProcessingGroup with an existing name should not modify its contents or recreate it.");
            Assert.That(bucketsAfterSecondCall[groupName], Is.SameAs(initialBucketState),
                        "Calling CreateProcessingGroup with an existing name should return the identical bucket instance.");
        }

        /// <summary>
        /// Tests that providing a null, empty, or whitespace name for a processing group throws an ArgumentException.
        /// </summary>
        [Test]
        public void CreateProcessingGroup_InvalidName_ThrowsArgumentException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateProcessingGroup(null), "Expected ArgumentException for null group name.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateProcessingGroup(""), "Expected ArgumentException for empty group name.");
            Assert.Throws<ArgumentException>(() => FSM_API.Create.CreateProcessingGroup("   "), "Expected ArgumentException for whitespace group name.");
        }

        /// <summary>
        /// Tests that calling CreateProcessingGroup on an existing group does not create a new one.
        /// </summary>
        [Test]
        public void CreateProcessingGroup_ExistingGroup_DoesNotCreateNew()
        {
            // Arrange
            string groupName = "ExistingGroup";
            FSM_API.Create.CreateProcessingGroup(groupName);
            var initialCount = FSM_API.Internal.ProcessingGroupCount;

            // Act
            FSM_API.Create.CreateProcessingGroup(groupName);

            // Assert
            Assert.That(FSM_API.Internal.ProcessingGroupCount, Is.EqualTo(initialCount), "No new group should be created when one with the same name already exists.");
        }


    }


    

}
