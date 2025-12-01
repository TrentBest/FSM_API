using NUnit.Framework;

using System.Collections.Generic;

using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests.Engine
{
    [TestFixture]
    public class FSM_API_Engine_Tests
    {
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        [Test]
        public void Engine_Respects_ProcessRate_Throttling()
        {
            // ARRANGE
            // Create an FSM that only updates every 3 ticks
            int processRate = 3;
            int updateCount = 0;
            string fsmName = "ThrottledFSM";

            FSM_API.Create.CreateFiniteStateMachine(fsmName, processRate: processRate)
                .State("Idle",null,
                    onUpdate: (ctx) => updateCount++, null) // Increment counter every time Update actually runs
                .BuildDefinition();

            FSM_API.Create.CreateInstance(fsmName, new MockContext());

            // ACT
            // Simulate 10 frames of the engine
            for (int i = 0; i < 10; i++)
            {
                FSM_API.Interaction.Update();
            }

            // ASSERT
            // Rate 3 means it runs on tick 1, 4, 7, 10... (or similar, depending on exact counter logic)
            // It should definitely NOT be 10. 
            // Based on your logic: Counter starts at 3. 
            // Tick 1: Counter(2) -> Skip. 
            // Tick 2: Counter(1) -> Skip. 
            // Tick 3: Counter(0) -> Reset to 3, RUN.
            // So in 10 ticks, it should run 3 times (ticks 3, 6, 9).
            Assert.That(updateCount, Is.EqualTo(3),
                $"FSM with rate {processRate} should have updated 3 times in 10 ticks, but updated {updateCount} times.");
        }

        [Test]
        public void Engine_Handles_Zombie_Instance_Gracefully()
        {
            // THE "ZOMBIE" SCENARIO:
            // Instance A and Instance B are in the update list.
            // Instance A updates first and DESTROYS Instance B.
            // Instance B should NOT update this frame (it is dead).

            // NOTE: This test will FAIL if your TickAll loop iterates a snapshot 
            // without checking if the instance is still valid/alive.

            // ARRANGE
            bool zombieUpdated = false;
            var ctxA = new MockContext { Name = "Killer" };
            var ctxB = new MockContext { Name = "Victim" };

            // Define FSM
            FSM_API.Create.CreateFiniteStateMachine("ZombieTestFSM", processRate: -1, "Update")
                .State("Active", null,
                    onUpdate: (ctx) =>
                    {
                        if (ctx == ctxA)
                        {
                            // A kills B
                            var handleB = FSM_API.Interaction.GetInstance("ZombieTestFSM", ctxB, "Update");
                            if (handleB != null) FSM_API.Interaction.DestroyInstance(handleB);
                        }
                        else if (ctx == ctxB)
                        {
                            // B records if it ran (it shouldn't!)
                            zombieUpdated = true;
                        }
                    }, null)
                .BuildDefinition();

            // Create instances. Ensure A is processed before B (List order usually follows creation order)
            FSM_API.Create.CreateInstance("ZombieTestFSM", ctxA);
            FSM_API.Create.CreateInstance("ZombieTestFSM", ctxB);

            // ACT
            FSM_API.Interaction.Update();

            // ASSERT
            Assert.That(zombieUpdated, Is.False,
                "CRITICAL: The victim instance executed its Update logic AFTER being destroyed in the same frame.");
        }

        [Test]
        public void Engine_AutoShutdown_On_Invalid_Context()
        {
            // Verify the API's safety net: automatic removal of instances with invalid contexts.

            // ARRANGE
            var context = new MockContext();
            string fsmName = "SafetyNetFSM";

            FSM_API.Create.CreateFiniteStateMachine(fsmName).WithProcessRate(-1).BuildDefinition();
            var handle = FSM_API.Create.CreateInstance(fsmName, context);

            // Run once to prove it's alive
            FSM_API.Interaction.Update();
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1));

            // ACT
            // Corrupt the context
            context.IsValid = false;

            // Run update loop - this should detect the invalid context and trigger error/cleanup
            FSM_API.Interaction.Update();
            FSM_API.Interaction.Update(); // Needs 2 ticks? 1 for error detection, 1 for deferred cleanup?

            // ASSERT
            // We expect the handle to be removed because it became invalid
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(0),
                "Engine should have automatically destroyed the instance with invalid context.");
        }

        private class MockContext : IStateContext
        {
            public bool IsValid { get; set; } = true;
            public string Name { get; set; }
        }
    }
}