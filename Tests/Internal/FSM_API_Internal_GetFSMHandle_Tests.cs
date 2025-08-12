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
    public class FSM_API_Internal_GetFSMHandle_Tests
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
        public void GetFSMHandle_Test()
        {
            FSMTestContext context = new FSMTestContext();
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", -1, "Update").BuildDefinition();
            FSM_API.Create.CreateInstance("TestFSM", context, "Update");
            var handle = FSM_API.Internal.GetFSMHandle("TestFSM", context, "Update");
            Assert.That(handle, Is.Not.Null, "GetFSMHandle should return null when no FSM is defined.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFSMHandle_InvalidFSM_Test()
        {
            FSMTestContext context = new FSMTestContext();
            var handle = FSM_API.Internal.GetFSMHandle("InvalidFSM", context, "Update");
            Assert.That(handle, Is.Null, "GetFSMHandle should return null when the FSM is not defined.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFSMHandle_InvalidContext_Test()
        {
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", -1, "Update").BuildDefinition();
            var handle = FSM_API.Internal.GetFSMHandle("TestFSM", null, "Update");
            Assert.That(handle, Is.Null, "GetFSMHandle should return null when the context is null.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFSMHandle_InvalidProcessingGroup_Test()
        {
            FSMTestContext context = new FSMTestContext();
            FSM_API.Create.CreateFiniteStateMachine("TestFSM", -1, "Update").BuildDefinition();
            var handle = FSM_API.Internal.GetFSMHandle("TestFSM", context, "InvalidGroup");
            Assert.That(handle, Is.Null, "GetFSMHandle should return null when the processing group is invalid.");
        }
    }
}
