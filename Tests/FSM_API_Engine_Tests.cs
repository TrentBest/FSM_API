using NUnit.Framework;

using System.Collections.Generic;

using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.Tests.Engine
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Engine_Tests
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Engine_Invalid_Context_Suspends_And_Can_Resume_Same_Handle()
        {
            // Invalidating pauses execution; it does not destroy or unregister the instance.
            var context = new MockContext();
            string fsmName = "ReusableFSM";
            int updateCount = 0;

            FSM_API.Create.CreateFiniteStateMachine(fsmName, processRate: -1, processingGroup: "Update")
                .State("Active", null, onUpdate: _ => updateCount++, null)
                .BuildDefinition();
            var handle = FSM_API.Create.CreateInstance(fsmName, context);

            FSM_API.Interaction.Update();
            Assert.That(updateCount, Is.EqualTo(1));
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1));

            context.IsValid = false;
            FSM_API.Interaction.Update();
            FSM_API.Interaction.Update();
            handle.Update();
            handle.EvaluateConditions();

            Assert.That(updateCount, Is.EqualTo(1), "Scheduled and manual execution must skip an invalid context.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1),
                "Invalidating must preserve the registered handle for reuse.");
            Assert.That(handle.IsValid, Is.False);

            context.IsValid = true;
            FSM_API.Interaction.Update();

            Assert.That(updateCount, Is.EqualTo(2), "Revalidating should resume the same handle.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1));
            Assert.That(FSM_API.Interaction.GetInstance(fsmName, context, "Update"), Is.SameAs(handle));
        }

        [Test]
        public void Destroying_One_Handle_Preserves_Shared_Context_And_Other_Handle()
        {
            var context = new MockContext();
            int firstUpdates = 0;
            int secondUpdates = 0;
            int firstExits = 0;

            FSM_API.Create.CreateFiniteStateMachine("SharedContextA", processRate: -1, processingGroup: "Update")
                .State("Active", null, onUpdate: _ => firstUpdates++, onExit: _ => firstExits++)
                .BuildDefinition();
            FSM_API.Create.CreateFiniteStateMachine("SharedContextB", processRate: -1, processingGroup: "Update")
                .State("Active", null, onUpdate: _ => secondUpdates++, onExit: null)
                .BuildDefinition();

            var firstHandle = FSM_API.Create.CreateInstance("SharedContextA", context);
            var secondHandle = FSM_API.Create.CreateInstance("SharedContextB", context);

            FSM_API.Interaction.Update();
            Assert.That(firstUpdates, Is.EqualTo(1));
            Assert.That(secondUpdates, Is.EqualTo(1));

            FSM_API.Interaction.DestroyInstance(firstHandle);

            Assert.That(context.IsValid, Is.True,
                "Destroying an FSM handle must not invalidate the caller-owned shared context.");
            Assert.That(firstExits, Is.EqualTo(1),
                "Explicit handle destruction should still execute the target state's exit callback.");
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1),
                "Only the explicitly targeted handle should be unregistered.");

            FSM_API.Interaction.Update();

            Assert.That(firstUpdates, Is.EqualTo(1),
                "The destroyed handle must no longer be scheduled.");
            Assert.That(secondUpdates, Is.EqualTo(2),
                "A different live handle sharing the same context must continue processing.");
            Assert.That(context.IsValid, Is.True);
            Assert.That(FSM_API.Internal.TotalFsmHandleCount, Is.EqualTo(1));
        }

        /// <summary>
        /// 
        /// </summary>
        private class MockContext : IStateContext
        {
            /// <summary>
            /// 
            /// </summary>
            public bool IsValid { get; set; } = true;
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
        }
    }
}