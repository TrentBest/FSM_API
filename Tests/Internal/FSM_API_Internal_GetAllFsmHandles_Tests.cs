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
    public class FSM_API_Internal_GetAllFsmHandles_Tests
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
        public void GetAllFsmHandles_InitiallyEmpty_Test()
        {
            // ACT
            var handles = FSM_API.Internal.GetAllFsmHandles();
            // ASSERT
            Assert.That(handles, Is.Not.Null, "GetAllFsmHandles should never return null.");
            Assert.That(handles, Is.Empty, "Handles should be empty after a hard reset.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllFsmHandles_WithFSMs_Test()
        {
            // ARRANGE
            string fsmName = "TestFSM";
            string groupName = "Update";
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            FSM_API.Create.CreateInstance(fsmName, new FSMTestContext(), groupName);
            // ACT
            var handles = FSM_API.Internal.GetAllFsmHandles().ToArray();
            // ASSERT
            Assert.That(handles, Is.Not.Null, "GetAllFsmHandles should not return null.");
            Assert.That(handles.Count, Is.EqualTo(1), "GetAllFsmHandles should return a list with one FSM handle.");
            Assert.That(handles[0].Name, Is.EqualTo(fsmName), "GetAllFsmHandles should return the correct FSM handle.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetAllFsmHandles_WithMultipleFSMs_Test()
        {
            // ARRANGE
            string fsmName1 = "TestFSM1";
            string fsmName2 = "TestFSM2";
            string groupName = "Update";
            FSM_API.Create.CreateFiniteStateMachine(fsmName1, -1, groupName).BuildDefinition();
            FSM_API.Create.CreateFiniteStateMachine(fsmName2, -1, groupName).BuildDefinition();
            FSM_API.Create.CreateInstance(fsmName1, new FSMTestContext(), groupName);
            FSM_API.Create.CreateInstance(fsmName2, new FSMTestContext(), groupName);
            // ACT
            var handles = FSM_API.Internal.GetAllFsmHandles().ToArray();
            // ASSERT
            Assert.That(handles, Is.Not.Null, "GetAllFsmHandles should not return null.");
            Assert.That(handles.Count, Is.EqualTo(2), "GetAllFsmHandles should return a list with two FSM handles.");
            Assert.That(handles.Any(h => h.Name == fsmName1), Is.True, "GetAllFsmHandles should contain the first FSM handle.");
            Assert.That(handles.Any(h => h.Name == fsmName2), Is.True, "GetAllFsmHandles should contain the second FSM handle.");
        }

    }
}
