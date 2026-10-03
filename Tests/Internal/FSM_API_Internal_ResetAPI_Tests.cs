using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FsmApi.Internal;
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
            FsmApi.Internal.ResetAPI(true);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SoftResetAPI_RemovesProcessingGroupTest()
        {
            FsmApi.Create.CreateProcessingGroup("TestPG");

            FsmApi.Internal.ResetAPI();
            var count = FsmApi.Internal.GetProcessingGroups().Count;
            Console.WriteLine($"count:  {count}");
            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SoftResetAPI_RemovesFSMTest()
        {
            FsmApi.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();

            FsmApi.Internal.ResetAPI();

            var count = FsmApi.Internal.GetBuckets().Count;
            Console.WriteLine($"count:  {count}");

            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SoftResetAPI_RemovesFSMInstanceTest()
        {
            FsmApi.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();
            var handle = FsmApi.Create.CreateInstance("TestFSM", new FSMTestContext());
            FsmApi.Internal.ResetAPI();

            Assert.That(FsmApi.Internal.TotalFsmHandleCount, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HardResetAPI_RemovesProcessingGroupTest()
        {
            FsmApi.Create.CreateProcessingGroup("TestPG");

            FsmApi.Internal.ResetAPI(true);
            var count = FsmApi.Internal.GetProcessingGroups().Count;
            Console.WriteLine($"count:  {count}");
            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HardResetAPI_RemovesFSMTest()
        {
            FsmApi.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();

            FsmApi.Internal.ResetAPI(true);

            var count = FsmApi.Internal.GetBuckets().Count;
            Console.WriteLine($"count:  {count}");

            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void HardResetAPI_RemovesFSMInstanceTest()
        {
            FsmApi.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();
            var handle = FsmApi.Create.CreateInstance("TestFSM", new FSMTestContext());
            FsmApi.Internal.ResetAPI(true);

            Assert.That(FsmApi.Internal.TotalFsmHandleCount, Is.EqualTo(0));
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
        /// Context identifier derived from the test context name.
        /// </summary>
        public int Context_ID => Name?.GetHashCode() ?? 0;
        /// <summary>
        /// 
        /// </summary>
        public bool HasEntered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool HasEnteredCurrentState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TestData { get; set; }
    }
}
