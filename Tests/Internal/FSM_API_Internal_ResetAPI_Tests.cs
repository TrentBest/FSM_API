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
    public class FSM_API_Internal_ResetAPI_Tests
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
        public void SoftResetAPI_RemovesProcessingGroupTest()
        {
            FSM_API.Create.CreateProcessingGroup("TestPG");

            FSM_API.Internal.ResetAPI();
            var count = FSM_API.Internal.GetProcessingGroups().Count;
            Console.WriteLine($"count:  {count}");
            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SoftResetAPI_RemovesFSMTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();

            FSM_API.Internal.ResetAPI();

            var count = FSM_API.Internal.GetBuckets().Count;
            Console.WriteLine($"count:  {count}");

            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SoftResetAPI_RemovesFSMInstanceTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext());
            FSM_API.Internal.ResetAPI();

            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HardResetAPI_RemovesProcessingGroupTest()
        {
            FSM_API.Create.CreateProcessingGroup("TestPG");

            FSM_API.Internal.ResetAPI(true);
            var count = FSM_API.Internal.GetProcessingGroups().Count;
            Console.WriteLine($"count:  {count}");
            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HardResetAPI_RemovesFSMTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();

            FSM_API.Internal.ResetAPI(true);

            var count = FSM_API.Internal.GetBuckets().Count;
            Console.WriteLine($"count:  {count}");

            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HardResetAPI_RemovesFSMInstanceTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext());
            FSM_API.Internal.ResetAPI(true);

            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FSMTestContext : IStateContext
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsValid { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = "TestContext";
        /// <summary>
        /// 
        /// </summary>
        public bool HasEntered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool HasEnteredCurrentState { get; set; }
    }
}
