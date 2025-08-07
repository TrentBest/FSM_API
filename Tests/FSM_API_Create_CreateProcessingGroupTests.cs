namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public class FSM_API_Create_CreateProcessingGroupTests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

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
    }


    

}
