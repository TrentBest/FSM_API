namespace TheSingularityWorkshop.FSM_API
{
    [TestFixture]
    public class FSM_API_CreateTests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Interaction.ResetAPI(true);
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

       
    }


    //Processing group with a null string
    //Processing group with a string.Empty
    //Processing group with a max length string

    //Creating an FSM (here we are only testing the method which returns the FSMBuider... we will test the builder in it's own test file.
    //Creating an FSM without a defined ProcessingGroup, defines the ProcessingGroup to add the FSM.
    //Creating an FSM with a defined ProcessingGroup is added to a bucket in that processing group

    //Creating a Handle of a nonexistent FSM puts the handle into our DefaultFSM
    //Creating a Handle with a null context ...
    //Creating a handle without a processing group defined should default itself to the processing group of it's FSM Definition
    //Creating a handle with a processing group passed creates a handle (and a clone of it's definition for this group)

    //... anything else you can think about

}
