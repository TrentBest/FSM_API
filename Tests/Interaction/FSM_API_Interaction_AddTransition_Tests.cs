using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FSM_API.Internal;
using TheSingularityWorkshop.FSM_API.Tests;
using TheSingularityWorkshop.FSM_API.Tests.Internal;


namespace TheSingularityWorkshop.FSM_API.Tests.Interaction
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Interaction_AddTransition_Tests
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
        public void AddTransition_Succeeds()
        {
            // ARRANGE
            var fsmName = "TestFSM";
            var processingGroup = "Update";
            var stateA = "StateA";
            var stateB = "StateB";
            FSM_API.Create.CreateProcessingGroup(processingGroup);
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
            .State(stateA, null, null, null)
            .State(stateB, null, null, null)
            .BuildDefinition();

            Assert.That(FSM_API.Interaction.GetFSMDefinition(fsmName, processingGroup).GetAllTransitions().Count, Is.EqualTo(0), "FSM should initially have no transitions.");
            // ACT
            FSM_API.Interaction.AddTransition(fsmName, stateA, stateB, TestCondition, processingGroup);

            // ASSERT
            Assert.That(FSM_API.Interaction.GetFSMDefinition(fsmName, processingGroup).GetAllTransitions().Count, Is.EqualTo(1), "FSM should have one transition after adding one.");
        }

        private bool TestCondition(IStateContext context)
        {
            if(context is FSMTestContext testContext)
            {
                return testContext.TestData == 1;
            }
            return false;
        }
    }
}
