using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Tests.Internal;

namespace TheSingularityWorkshop.FSM_API.Tests.Interaction
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Interaction_AddStateToFSM_Tests
    {
        private const string FsmName = "TestFSM";
        private const string GroupName = "Update";
        private const string StateA = "StateA";
        private const string StateB = "StateB";
       
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
        public void AddStateToFSM_Succeeds_WhenFSMExists()
        {
            FSM_API.Create.CreateFiniteStateMachine(FsmName, processingGroup: GroupName)
                .State(StateA, null, null, null)
                .BuildDefinition();

            Assert.That(FSM_API.Interaction.GetFSMDefinition(FsmName, GroupName).GetAllStates().Count, Is.EqualTo(1));

            FSM_API.Interaction.AddStateToFSM(FsmName, StateB, null, null, null, GroupName);

            Assert.That(FSM_API.Interaction.GetFSMDefinition(FsmName, GroupName).GetAllStates().Count, Is.EqualTo(2));
            Assert.That(FSM_API.Interaction.GetFSMDefinition(FsmName, GroupName).HasState(StateB), Is.True);
        }
       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateName"></param>
        [TestCase("")]
        [TestCase("   ")]
        [Test]
        public void AddStateToFSM_InvalidStateName_ThrowsArgumentException(string stateName)
        {
            FSM_API.Create.CreateFiniteStateMachine(FsmName, processingGroup: GroupName)
                .State(StateA, null, null, null)
                .BuildDefinition();

            Assert.Throws<ArgumentException>(() => FSM_API.Interaction.AddStateToFSM(FsmName, stateName, null, null, null, GroupName));
        }

       
    }
}