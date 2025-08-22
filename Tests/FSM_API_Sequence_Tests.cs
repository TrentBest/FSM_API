using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API.Tests.Interaction;
using TheSingularityWorkshop.FSM_API.Tests.Internal;

using TestContext = TheSingularityWorkshop.FSM_API.Tests.Interaction.TestContext;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Sequence_Tests
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
        public void FSM_Sequences_Correctly()
        {
            FSM_API.Create.CreateProcessingGroup("TestGroup");

            FSM_API.Create.CreateFiniteStateMachine("TestFSM", -1, "TestGroup")
                .State("StateA", OnEnterStateA, OnUpdateStateA, OnExitStateA)
                .State("StateB", OnEnterStateB, OnUpdateStateB, OnExistStateB)
                .Transition("StateA", "StateB", ShouldTransitionToB)
                .Transition("StateB", "StateA", ShouldTransitionToA)
                .BuildDefinition();

            var handle = FSM_API.Create.CreateInstance("TestFSM", new FSMTestContext(), "TestGroup");

            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");

            Assert.That(handle.CurrentState, Is.EqualTo("StateB"));
            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            FSM_API.Interaction.Update("TestGroup");
            Assert.That(handle.CurrentState, Is.EqualTo("StateA"));
        }

        private void OnEnterStateA(IStateContext context)
        {
            Console.WriteLine("Entering State A");
            if (context is FSMTestContext tc)
            {
                tc.TestData = 5;
            }
        }

        private void OnUpdateStateA(IStateContext context)
        {
            Console.WriteLine("Updating State A");
            if (context is FSMTestContext tc)
            {
                tc.TestData--;
                Console.WriteLine($"Updated State A:  {tc.TestData}");
            }
        }

        private void OnExitStateA(IStateContext context)
        {
            Console.WriteLine("Exiting State A");
        }

        private void OnEnterStateB(IStateContext context)
        {
            Console.WriteLine("Entering State B");
            if (context is FSMTestContext tc)
            {
                tc.TestData = 5;
            }
        }

        private void OnUpdateStateB(IStateContext context)
        {
            Console.WriteLine("Updating State B");
            if (context is FSMTestContext tc)
            {
                tc.TestData--;
                Console.WriteLine($"Updated State A:  {tc.TestData}");
            }
        }

        private void OnExistStateB(IStateContext context)
        {
            Console.WriteLine("Exiting State B");

        }

        private bool ShouldTransitionToB(IStateContext context)
        {
            bool result = false;
            if (context is FSMTestContext tc)
            {
                result = tc.TestData <= 0;
                Console.WriteLine($"{tc.TestData} <= 0:  {result}");
            }
            Console.WriteLine($"Evaluating if Should Transition To B:  {result}");
            return result;
        }

        private bool ShouldTransitionToA(IStateContext context)
        {
            bool result = false;
            if (context is FSMTestContext tc)
            {
                result = tc.TestData <= 0;
                Console.WriteLine($"{tc.TestData} <= 0:  {result}");
            }
            Console.WriteLine($"Evaluating if Should Transition To B:  {result}");
            return result;
        }
    }
}
