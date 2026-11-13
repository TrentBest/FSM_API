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
    public class FSM_API_Internal_DestroyInsance_Tests
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
        public void DestroyInstance_InitiallyEmpty_Test()
        {
            // ARRANGE
            string fsmName = "TestFSM";
            string groupName = "Update";
            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            var instance = FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), groupName);
            // ACT
            FsmApi.Internal.DestroyInstance(instance);
            // ASSERT
            Assert.That(FsmApi.Internal.GetAllFsmHandles(), Is.Empty, "GetAllFsmHandles should be empty after destroying the instance.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void DestroyInstance_WithMultipleInstances_Test()
        {
            // ARRANGE
            string fsmName = "TestFSM";
            string groupName = "Update";
            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            var instance1 = FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), groupName);
            var instance2 = FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), groupName);
            // ACT
            FsmApi.Internal.DestroyInstance(instance1);
            // ASSERT
            Assert.That(FsmApi.Internal.GetAllFsmHandles().Count(), Is.EqualTo(1), "GetAllFsmHandles should contain one instance after destroying one.");
            Assert.That(FsmApi.Internal.GetAllFsmHandles().First().Definition.Name, Is.EqualTo(fsmName), "The remaining instance should still be the same FSM.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void DestroyInstance_NonExistentInstance_Test()
        {
            // ARRANGE
            string fsmName = "TestFSM";
            string groupName = "Update";
            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            var instance = FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), groupName);
            FsmApi.Internal.DestroyInstance(instance); // Destroy the instance first
            // ACT & ASSERT
            Assert.DoesNotThrow(() => FsmApi.Internal.DestroyInstance(instance), "Destroying an already destroyed instance should throw an exception.");
            Assert.That(FsmApi.Internal.GetAllFsmHandles(), Is.Empty, "GetAllFsmHandles should still be empty after attempting to destroy a non-existent instance.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void DestroyInstance_WithManyInstances_Test()
        {
            // ARRANGE
            string fsmName = "TestFSM";
            string groupName = "Update";
            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, groupName).BuildDefinition();
            var instances = new List<FSMHandle>();
            for (int i = 0; i < 5; i++)
            {
                instances.Add(FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), groupName));
            }
            // ACT
            foreach (var instance in instances)
            {
                FsmApi.Internal.DestroyInstance(instance);
            }
            // ASSERT
            Assert.That(FsmApi.Internal.GetAllFsmHandles(), Is.Empty, "GetAllFsmHandles should be empty after destroying all instances.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void DestroyInstance_WithMultipleFSMsAndProcessGroups_Test()
        {
            FsmApi.Create.CreateProcessingGroup("Group1");
            FsmApi.Create.CreateProcessingGroup("Group2");
            string fsmName = "FSM";
            FSMHandle handle = null;
            foreach (var group in FsmApi.Internal.GetProcessingGroupNames())
            {
                for (int i = 0; i < 5; i++)
                {
                    FsmApi.Create.CreateFiniteStateMachine($"{fsmName}_{i}", -1, group).BuildDefinition();
                    for (int j = 0; j < 100; j++)
                    {
                        handle = FsmApi.Create.CreateInstance($"{fsmName}_{i}", new FSMTestContext(), group);
                    }
                }
            }
            var allHandles = FsmApi.Internal.GetAllFsmHandles().Count();
            // Act
            FsmApi.Internal.DestroyInstance(handle);

            var reducedHandles = FsmApi.Internal.GetAllFsmHandles().Count();

            //Assert
            
            Assert.That(reducedHandles, Is.EqualTo(allHandles-1), "GetAllFsmHandles should contain one less instance after destroying one.");

        }
    }
}
