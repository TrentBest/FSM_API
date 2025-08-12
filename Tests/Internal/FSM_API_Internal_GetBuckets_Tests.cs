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
    public class FSM_API_Internal_GetBuckets_Tests
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
            Assert.That(buckets[groupName], Is.Not.Null, $"The bucket for '{groupName}' should not be null.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReflectsFSMDefinitionCreation()
        {
            // ARRANGE
            var groupName = "TestGroup";
            FSM_API.Create.CreateProcessingGroup(groupName);
            string fsmName = "TestFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            // ACT
            var buckets = FSM_API.Internal.GetBuckets();
            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), $"Buckets should contain the processing group '{groupName}'.");
            Assert.That(buckets[groupName], Contains.Key(fsmName), $"The bucket for '{groupName}' should contain the FSM '{fsmName}'.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReflectsMultipleFSMDefinitions()
        {
            // ARRANGE
            var groupName = "TestGroup";
            FSM_API.Create.CreateProcessingGroup(groupName);
            string fsmName1 = "TestFSM1";
            string fsmName2 = "TestFSM2";
            FSM_API.Create.CreateFiniteStateMachine(fsmName1, -1, groupName).BuildDefinition();
            FSM_API.Create.CreateFiniteStateMachine(fsmName2, -1, groupName).BuildDefinition();
            // ACT
            var buckets = FSM_API.Internal.GetBuckets();
            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), $"Buckets should contain the processing group '{groupName}'.");
            Assert.That(buckets[groupName], Contains.Key(fsmName1), $"The bucket for '{groupName}' should contain the FSM '{fsmName1}'.");
            Assert.That(buckets[groupName], Contains.Key(fsmName2), $"The bucket for '{groupName}' should contain the FSM '{fsmName2}'.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReflectsFSMInstanceCreation()
        {
            // ARRANGE
            var groupName = "TestGroup";
            FSM_API.Create.CreateProcessingGroup(groupName);
            string fsmName = "TestFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            FSMTestContext context = new FSMTestContext();
            FSM_API.Create.CreateInstance(fsmName, context, groupName);
            // ACT
            var buckets = FSM_API.Internal.GetBuckets();
            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), $"Buckets should contain the processing group '{groupName}'.");
            Assert.That(buckets[groupName], Contains.Key(fsmName), $"The bucket for '{groupName}' should contain the FSM '{fsmName}'.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReflectsMultipleFSMInstances()
        {
            // ARRANGE
            var groupName = "TestGroup";
            FSM_API.Create.CreateProcessingGroup(groupName);
            string fsmName = "TestFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            FSMTestContext context1 = new FSMTestContext();
            FSMTestContext context2 = new FSMTestContext();
            FSM_API.Create.CreateInstance(fsmName, context1, groupName);
            FSM_API.Create.CreateInstance(fsmName, context2, groupName);
            // ACT
            var buckets = FSM_API.Internal.GetBuckets();
            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), $"Buckets should contain the processing group '{groupName}'.");
            Assert.That(buckets[groupName], Contains.Key(fsmName), $"The bucket for '{groupName}' should contain the FSM '{fsmName}'.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetBuckets_ReflectsFSMInstanceWithMultipleContexts()
        {
            // ARRANGE
            var groupName = "TestGroup";
            FSM_API.Create.CreateProcessingGroup(groupName);
            string fsmName = "TestFSM";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            FSMTestContext context1 = new FSMTestContext();
            FSMTestContext context2 = new FSMTestContext();
            FSM_API.Create.CreateInstance(fsmName, context1, groupName);
            FSM_API.Create.CreateInstance(fsmName, context2, groupName);
            // ACT
            var buckets = FSM_API.Internal.GetBuckets();
            // ASSERT
            Assert.That(buckets, Contains.Key(groupName), $"Buckets should contain the processing group '{groupName}'.");
            Assert.That(buckets[groupName], Contains.Key(fsmName), $"The bucket for '{groupName}' should contain the FSM '{fsmName}'.");
        }
    }
}
