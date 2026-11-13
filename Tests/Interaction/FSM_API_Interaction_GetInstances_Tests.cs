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
    public class FSM_API_Interaction_GetInstances_Tests
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
        public void GetInstances_FSMExistsWithMultipleInstancesInDefaultGroup_ReturnsAllInstances()
        {
            // Arrange
            string fsmName = "PlayerFSM";
            FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            List<FSMHandle> expectedHandles = new List<FSMHandle>
            {
                FsmApi.Create.CreateInstance(fsmName, new Tests.Internal.FSMTestContext()),
                FsmApi.Create.CreateInstance(fsmName, new Tests.Internal.FSMTestContext()),
                FsmApi.Create.CreateInstance(fsmName, new Tests.Internal.FSMTestContext())
            };

            // Act
            IReadOnlyList<FSMHandle> actualHandles = FsmApi.Interaction.GetInstances(fsmName);

            // Assert
            Assert.That(actualHandles, Is.EqualTo(expectedHandles), "Returned list should not be null.");
            Assert.That(actualHandles.Count, Is.EqualTo(expectedHandles.Count), "Expected count of instances does not match.");
            
            Assert.That(actualHandles, Is.EquivalentTo(expectedHandles), "Returned instances should match the expected handles.");
            Assert.That(FsmApi.Internal.TotalFsmHandleCount, Is.EqualTo(3), "Total FSM handle count should reflect created instances.");
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void GetInstances_FSMExistsWithMultipleInstancesInCustomGroup_ReturnsAllInstances()
        //{
        //    // Arrange
        //    string fsmName = "EnemyAI";
        //    string customProcessingGroup = "EnemyUpdate";
        //    FsmApi.Create.CreateFiniteStateMachine(fsmName, processingGroup: customProcessingGroup).BuildDefinition();
        //    List<FSMHandle> expectedHandles = new List<FSMHandle>
        //    {
        //        FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), customProcessingGroup),
        //        FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), customProcessingGroup)
        //    };
        //    // Create an instance in a different group to ensure isolation
        //    FsmApi.Create.CreateFiniteStateMachine("OtherFSM", processingGroup: "OtherGroup").BuildDefinition();
        //    FsmApi.Create.CreateInstance("OtherFSM", new FSMTestContext(), "OtherGroup");


        //    // Act
        //    IReadOnlyList<FSMHandle> actualHandles = FsmApi.Interaction.GetInstances(fsmName, customProcessingGroup);

        //    // Assert
        //    Assert.That(actualHandles, Is.True, "Returned list should not be null.");
        //    Assert.That(actualHandles.Count, Is.EqualTo(expectedHandles.Count), "Expected count of instances does not match.");
        //    Assert.That(expectedHandles, Is.EquivalentTo(actualHandles), "Returned instances should match the expected handles in the custom group.");
        //    Assert.That(FsmApi.Internal.TotalFsmHandleCount, Is.EqualTo(3), "Total FSM handle count should reflect all instances created across groups.");
        //}


        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void GetInstances_FSMExistsWithSingleInstance_ReturnsSingleInstance()
        //{
        //    // Arrange
        //    string fsmName = "DoorFSM";
        //    FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
        //    FSMHandle expectedHandle = FsmApi.Create.CreateInstance(fsmName, new FSMTestContext());

        //    // Act
        //    IReadOnlyList<FSMHandle> actualHandles = FsmApi.Interaction.GetInstances(fsmName);

        //    // Assert
        //    Assert.That(actualHandles, Is.True, "Returned list should not be null.");
        //    Assert.That(actualHandles.Count, Is.EqualTo(1), "Expected exactly one FSM instance.");
        //    Assert.That(actualHandles.First(), Is.EqualTo(expectedHandle), "The returned handle should be the expected instance.");
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //[Test]
        //public void GetInstances_FSMExistsButHasNoInstances_ReturnsEmptyList()
        //{
        //    // Arrange
        //    string fsmName = "EmptyFSM";
        //    FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition(); // Define the FSM, but don't create instances

        //    // Act
        //    IReadOnlyList<FSMHandle> actualHandles = FsmApi.Interaction.GetInstances(fsmName);

        //    // Assert
        //    Assert.That(actualHandles, Is.Not.Null, "Returned list should not be null.");
        //    Assert.That(actualHandles, Is.True, "Expected an empty list when FSM definition exists but has no instances.");
        //    Assert.That(FsmApi.Internal.TotalFsmHandleCount, Is.EqualTo(0), "Total FSM handle count should still be zero.");
        //}

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_ReturnedListIsReadOnly()
        {
            // Arrange
            string fsmName = "ReadOnlyTestFSM";
            FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            var fsmDefinition = FsmApi.Interaction.GetFSMDefinition(fsmName);
            // Create some instances through your API, which GetInstances() will then retrieve.
            FsmApi.Create.CreateInstance(fsmName, new Tests.Internal.FSMTestContext());
            FsmApi.Create.CreateInstance(fsmName, new Tests.Internal.FSMTestContext());

            // Act
            // THIS IS WHERE YOU CALL YOUR API'S METHOD
            IReadOnlyList<FSMHandle> handles = FsmApi.Interaction.GetInstances(fsmName);

            // Assert
            Assert.That(handles, Is.Not.Null, "Returned collection should not be null.");

            // CORRECTED ASSERTION: Ensure it's NOT a mutable List<FSMHandle>
            Assert.That(handles, Is.Not.InstanceOf<List<FSMHandle>>(), "Returned collection should not be a mutable List<FSMHandle>.");

            // This assertion was incorrect, as IReadOnlyList<T> is an interface that List<T> implements.
            // Assert.That(handles, Is.Not.InstanceOf<IReadOnlyList<FSMHandle>>(), "Returned collection should not be a mutable List<FSMHandle>.");

            // Ensure it's not a FSMHandle array
            Assert.That(handles, Is.Not.InstanceOf<FSMHandle[]>(), "Returned collection should not be a FSMHandle array.");

            // Test for NotSupportedException if the underlying list is a mutable type wrapped as read-only.
            if (handles is IList<FSMHandle> mutableHandlesAttempt)
            {
                // Provide valid arguments for FSMHandle constructor for testing Add/Remove
                // Use the fsmDefinition that was created in Arrange
                var dummyContext = new Tests.Internal.FSMTestContext();
                var dummyHandle = new FSMHandle(fsmDefinition, dummyContext);

                Assert.Throws<NotSupportedException>(() => mutableHandlesAttempt.Add(dummyHandle), "Adding to the list should throw NotSupportedException.");
                Assert.Throws<NotSupportedException>(() => mutableHandlesAttempt.Clear(), "Clearing the list should throw NotSupportedException.");

                // Test removal if the collection has items
                if (handles.Any()) // Use .Any() from System.Linq to check if there are items
                {
                    Assert.Throws<NotSupportedException>(() => mutableHandlesAttempt.Remove(handles.First()), "Removing an existing item from the list should throw NotSupportedException.");
                }
            }
            else
            {
                // If it's not even castable to IList<FSMHandle>, it's even more read-only, which is perfectly fine.
                // This path indicates a truly immutable collection, where modification methods simply don't exist.
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_FSMExistsWithMultipleInstancesInCustomGroup_ReturnsAllInstances()
        {
            // Arrange
            string fsmName = "EnemyAI";
            string customProcessingGroup = "EnemyUpdate";
            FsmApi.Create.CreateProcessingGroup(customProcessingGroup);
            FsmApi.Create.CreateFiniteStateMachine(fsmName, processingGroup: customProcessingGroup).BuildDefinition();

            List<FSMHandle> expectedHandles = new List<FSMHandle>
    {
        FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), customProcessingGroup),
        FsmApi.Create.CreateInstance(fsmName, new FSMTestContext(), customProcessingGroup)
    };
            // Create an instance in a different group to ensure isolation
            FsmApi.Create.CreateFiniteStateMachine("OtherFSM", processingGroup: "OtherGroup").BuildDefinition();
            FsmApi.Create.CreateInstance("OtherFSM", new FSMTestContext(), "OtherGroup");

            // Act
            IReadOnlyList<FSMHandle> actualHandles = FsmApi.Interaction.GetInstances(fsmName, customProcessingGroup);

            // Assert
            Assert.That(actualHandles, Is.Not.Null, "Returned list should not be null.");
            Assert.That(actualHandles.Count, Is.EqualTo(expectedHandles.Count), "Expected count of instances does not match.");
            Assert.That(actualHandles, Is.EquivalentTo(expectedHandles), "Returned instances should match the expected handles in the custom group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_FSMExistsWithSingleInstance_ReturnsSingleInstance()
        {
            // Arrange
            string fsmName = "DoorFSM";
            FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            FSMHandle expectedHandle = FsmApi.Create.CreateInstance(fsmName, new FSMTestContext());

            // Act
            IReadOnlyList<FSMHandle> actualHandles = FsmApi.Interaction.GetInstances(fsmName);

            // Assert
            Assert.That(actualHandles, Is.Not.Null, "Returned list should not be null.");
            Assert.That(actualHandles.Count, Is.EqualTo(1), "Expected exactly one FSM instance.");
            Assert.That(actualHandles.First(), Is.EqualTo(expectedHandle), "The returned handle should be the expected instance.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_FSMExistsButHasNoInstances_ReturnsEmptyList()
        {
            // Arrange
            string fsmName = "EmptyFSM";
            FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition(); // Define the FSM, but don't create instances

            // Act
            IReadOnlyList<FSMHandle> actualHandles = FsmApi.Interaction.GetInstances(fsmName);

            // Assert
            Assert.That(actualHandles, Is.Not.Null, "Returned list should not be null.");
            Assert.That(actualHandles, Is.Empty, "Expected an empty list when FSM definition exists but has no instances.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_NullFsmName_ThrowsArgumentException()
        {
            // Arrange (no FSM definition needed for this test as validation happens first)
            string nullFsmName = string.Empty;
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FsmApi.Interaction.GetInstances(nullFsmName, validProcessingGroup),
                "Expected ArgumentException for null FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_EmptyFsmName_ThrowsArgumentException()
        {
            // Arrange
            string emptyFsmName = "";
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FsmApi.Interaction.GetInstances(emptyFsmName, validProcessingGroup),
                "Expected ArgumentException for empty FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_WhitespaceFsmName_ThrowsArgumentException()
        {
            // Arrange
            string whitespaceFsmName = "   ";
            string validProcessingGroup = "Update";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FsmApi.Interaction.GetInstances(whitespaceFsmName, validProcessingGroup),
                "Expected ArgumentException for whitespace FSM name.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_NullProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string nullProcessingGroup = string.Empty;
            FsmApi.Create.CreateFiniteStateMachine(validFsmName).BuildDefinition(); // Need a definition to avoid KeyNotFoundException first

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FsmApi.Interaction.GetInstances(validFsmName, nullProcessingGroup),
                "Expected ArgumentException for null processing group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_EmptyProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string emptyProcessingGroup = "";
            FsmApi.Create.CreateFiniteStateMachine(validFsmName).BuildDefinition();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FsmApi.Interaction.GetInstances(validFsmName, emptyProcessingGroup),
                "Expected ArgumentException for empty processing group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_WhitespaceProcessingGroup_ThrowsArgumentException()
        {
            // Arrange
            string validFsmName = "TestFSM";
            string whitespaceProcessingGroup = "   ";
            FsmApi.Create.CreateFiniteStateMachine(validFsmName).BuildDefinition();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FsmApi.Interaction.GetInstances(validFsmName, whitespaceProcessingGroup),
                "Expected ArgumentException for whitespace processing group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_FSMDefinitionNotFoundInGroup_ThrowsKeyNotFoundException()
        {
            // Arrange
            string existingFsmName = "SomeFSM";
            string existingGroup = "Update";
            string nonExistentFsmName = "NonExistentFSM";
            FsmApi.Create.CreateFiniteStateMachine(existingFsmName, processingGroup: existingGroup).BuildDefinition();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => FsmApi.Interaction.GetInstances(nonExistentFsmName, existingGroup),
                "Expected KeyNotFoundException when FSM definition is not found in the specified group.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetInstances_ProcessingGroupNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            string fsmName = "FSMInNonExistentGroup";
            string nonExistentGroup = "UnknownGroup";
            // No FSMs are defined, so this group won't exist

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => FsmApi.Interaction.GetInstances(fsmName, nonExistentGroup),
                "Expected KeyNotFoundException when the processing group itself does not exist.");
        }

       
        /// <summary>
        /// Tests that GetInstances returns a single handle when one exists for a given FSM name.
        /// </summary>
        [Test]
        public void GetInstances_ReturnsOneHandleWhenOneExists()
        {
            // Arrange
            string fsmName = "TestFSM";
            FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            var handle = FsmApi.Create.CreateInstance(fsmName, new FSMTestContext());

            // Act
            var handles = FsmApi.Interaction.GetInstances(fsmName);

            // Assert
            Assert.That(handles.Count(), Is.EqualTo(1), "Expected one handle to be returned.");
            Assert.That(handles.First(), Is.EqualTo(handle), "The correct handle should be returned.");
        }

        /// <summary>
        /// Tests that GetInstances returns all handles for a single FSM definition.
        /// </summary>
        [Test]
        public void GetInstances_ReturnsAllHandlesForSingleDefinition()
        {
            // Arrange
            string fsmName = "TestFSM";
            FsmApi.Create.CreateFiniteStateMachine(fsmName).BuildDefinition();
            FsmApi.Create.CreateInstance(fsmName, new FSMTestContext());
            FsmApi.Create.CreateInstance(fsmName, new FSMTestContext());
            FsmApi.Create.CreateInstance(fsmName, new FSMTestContext());

            // Act
            var handles = FsmApi.Interaction.GetInstances(fsmName);

            // Assert
            Assert.That(handles.Count(), Is.EqualTo(3), "Expected three handles to be returned.");
        }

        /// <summary>
        /// Tests that GetInstances returns handles from a specific processing group when one is specified.
        /// </summary>
        [Test]
        public void GetInstances_WithGroupName_ReturnsOnlyHandlesFromThatGroup()
        {
            // Arrange
            string fsmName1 = "FSMGroupA";
            string fsmName2 = "FSMGroupB";
            string groupA = "GroupA";
            string groupB = "GroupB";

            FsmApi.Create.CreateFiniteStateMachine(fsmName1, processingGroup: groupA).BuildDefinition();
            FsmApi.Create.CreateFiniteStateMachine(fsmName2, processingGroup: groupB).BuildDefinition();

            FsmApi.Create.CreateInstance(fsmName1, new FSMTestContext(), groupA);
            FsmApi.Create.CreateInstance(fsmName1, new FSMTestContext(), groupA);
            FsmApi.Create.CreateInstance(fsmName2, new FSMTestContext(), groupB);
            FsmApi.Create.CreateInstance(fsmName2, new FSMTestContext(), groupB);
            FsmApi.Create.CreateInstance(fsmName2, new FSMTestContext(), groupB);

            // Act
            var handlesInGroupA = FsmApi.Interaction.GetInstances(fsmName1, groupA);
            var handlesInGroupB = FsmApi.Interaction.GetInstances(fsmName2, groupB);

            // Assert
            Assert.That(handlesInGroupA.Count(), Is.EqualTo(2), "Expected two FSM handles from 'GroupA' to be returned.");
            Assert.That(handlesInGroupB.Count(), Is.EqualTo(3), "Expected three FSM handles from 'GroupB' to be returned.");
        }
    }
}
