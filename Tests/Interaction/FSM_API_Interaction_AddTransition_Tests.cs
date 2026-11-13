using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FsmApi.Internal;
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
            FsmApi.Internal.ResetAPI(true);
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
            FsmApi.Create.CreateProcessingGroup(processingGroup);
            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
            .State(stateA, null, null, null)
            .State(stateB, null, null, null)
            .BuildDefinition();

            Assert.That(FsmApi.Interaction.GetFSMDefinition(fsmName, processingGroup).GetAllTransitions().Count, Is.EqualTo(0), "FSM should initially have no transitions.");
            // ACT
            FsmApi.Interaction.AddTransition(fsmName, stateA, stateB, TestCondition, processingGroup);

            // ASSERT
            Assert.That(FsmApi.Interaction.GetFSMDefinition(fsmName, processingGroup).GetAllTransitions().Count, Is.EqualTo(1), "FSM should have one transition after adding one.");
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
