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
    public class FSM_API_Internal_GetBucket_Tests
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
        /// Tests that GetBucket returns the correct bucket when the FSM exists.
        /// </summary>
        [Test]
        public void GetBucket_SucceedsWithExistingFSM()
        {
            // Act
            var bucket = FSM_API.Internal.GetBucket(FsmName, ProcessGroup);

            // Assert
            Assert.That(bucket, Is.Not.Null, "The bucket should not be null.");
            Assert.That(bucket.Definition.Name, Is.EqualTo(FsmName), "The bucket should contain the correct FSM definition.");
        }

        /// <summary>
        /// Tests that GetBucket returns null for a non-existent FSM name.
        /// </summary>
        [Test]
        public void GetBucket_ReturnsNullForNonExistentFsm()
        {
            // Act
            var bucket = FSM_API.Internal.GetBucket("NonExistentFSM", ProcessGroup);

            // Assert
            Assert.That(bucket, Is.Null, "The bucket should be null for a non-existent FSM.");
        }

        /// <summary>
        /// Tests that GetBucket returns null for a non-existent processing group.
        /// </summary>
        [Test]
        public void GetBucket_ReturnsNullForNonExistentProcessingGroup()
        {
            // Act
            var bucket = FSM_API.Internal.GetBucket(FsmName, "NonExistentGroup");

            // Assert
            Assert.That(bucket, Is.Null, "The bucket should be null for a non-existent processing group.");
        }

              
    }
}
