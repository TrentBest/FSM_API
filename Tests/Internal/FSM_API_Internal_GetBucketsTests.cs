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
    public class FSM_API_Internal_GetBucketsTests
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
        public void GetBuckets_InitiallyEmpty()
        {
            // ACT
            var buckets = FSM_API.Internal.GetBuckets();

            // ASSERT
            Assert.That(buckets, Is.Not.Null, "GetBuckets should never return null.");
            Assert.That(buckets, Is.Empty, "Buckets should be empty after a hard reset.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReflectsProcessingGroupCreation()
        {
            // ARRANGE
            var groupName = "NewProcessingGroup";
            FSM_API.Create.CreateProcessingGroup(groupName);

            // ACT
            var buckets = FSM_API.Internal.GetBuckets();

            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), $"Buckets should contain the newly created processing group '{groupName}'.");
            Assert.That(buckets[groupName], Is.Empty, "The new processing group's inner dictionary should be empty as no FSMs are defined yet.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReflectsFSMDefinitionRegistration()
        {
            // ARRANGE
            var fsmName = "TestFSM";
            var groupName = "DefaultGroup"; // Default group for CreateFiniteStateMachine
            FSM_API.Create.CreateFiniteStateMachine(fsmName, 0, groupName).BuildDefinition();

            // ACT
            var buckets = FSM_API.Internal.GetBuckets();

            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), $"Buckets should contain the processing group '{groupName}'.");
            Assert.That(buckets[groupName], Contains.Key(fsmName), $"The '{groupName}' processing group should contain the FSM definition '{fsmName}'.");
            Assert.That(buckets[groupName][fsmName].Definition, Is.Not.Null, "The FSM definition in the bucket should not be null.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReturnsSameReference()
        {
            // ARRANGE
            // Add some data to ensure the dictionary is not empty
            FSM_API.Create.CreateFiniteStateMachine("FSM1", processingGroup: "GroupA").BuildDefinition();
            FSM_API.Create.CreateProcessingGroup("GroupB");

            // ACT
            var buckets1 = FSM_API.Internal.GetBuckets();
            var buckets2 = FSM_API.Internal.GetBuckets();

            // ASSERT
            Assert.That(buckets1, Is.SameAs(buckets2), "Calling GetBuckets multiple times should return the identical dictionary instance.");

            // Verify modification through returned reference affects internal state
            var newGroupName = "DynamicallyAddedGroup";
            buckets1.Add(newGroupName, new Dictionary<string, FSM_API.Internal.FsmBucket>());

            var bucketsAfterModification = FSM_API.Internal.GetBuckets();
            Assert.That(bucketsAfterModification, Contains.Key(newGroupName), "Modifying the returned dictionary should affect the internal state.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_NoFSMsInGroup_ReturnsEmptyInnerDictionary()
        {
            // ARRANGE
            var groupName = "EmptyGroup";
            FSM_API.Create.CreateProcessingGroup(groupName); // Ensure group exists but no FSMs are added to it

            // ACT
            var buckets = FSM_API.Internal.GetBuckets();

            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), "The processing group should exist.");
            Assert.That(buckets[groupName], Is.Empty, "The inner dictionary for a group with no FSMs should be empty.");
        }
    }
}
