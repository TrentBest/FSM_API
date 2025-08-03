using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    [TestFixture]
    public class FSM_API_Internal_ResetAPI_Tests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void SoftResetAPI_RemovesProcessingGroupTest()
        {
            FSM_API.Create.CreateProcessingGroup("TestPG");

            FSM_API.Internal.ResetAPI();
            var count = FSM_API.Internal.GetProcessingGroups().Count;
            Console.WriteLine($"count:  {count}");
            Assert.True(count == 0);
        }

        [Test]
        public void SoftResetAPI_RemovesFSMTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();

            FSM_API.Internal.ResetAPI();

            var count = FSM_API.Internal.GetBuckets().Count;
            Console.WriteLine($"count:  {count}");

            Assert.True(count == 0);
        }

        [Test]
        public void SoftResetAPI_RemovesFSMInstanceTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext());
            FSM_API.Internal.ResetAPI();

            Assert.True(FSM_API.Internal.TotalFsmHandleCount == 0);
        }

        [Test]
        public void HardResetAPI_RemovesProcessingGroupTest()
        {
            FSM_API.Create.CreateProcessingGroup("TestPG");

            FSM_API.Internal.ResetAPI(true);
            var count = FSM_API.Internal.GetProcessingGroups().Count;
            Console.WriteLine($"count:  {count}");
            Assert.True(count == 0);
        }

        [Test]
        public void HardResetAPI_RemovesFSMTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();

            FSM_API.Internal.ResetAPI(true);

            var count = FSM_API.Internal.GetBuckets().Count;
            Console.WriteLine($"count:  {count}");

            Assert.True(count == 0);
        }

        [Test]
        public void HardResetAPI_RemovesFSMInstanceTest()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM").BuildDefinition();
            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext());
            FSM_API.Internal.ResetAPI(true);

            Assert.True(FSM_API.Internal.TotalFsmHandleCount == 0);
        }
    }

    public class FSMTestContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "TestContext";
        public bool HasEntered { get; set; }
    }
}
