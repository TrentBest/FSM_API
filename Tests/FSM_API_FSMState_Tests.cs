using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_FSMState_Tests
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
        public void State_Creates_Successfully()
        {
            FSMState state = new FSMState("TestState", null, null, null);

            Assert.That(state, Is.Not.Null, "State shouldn't be null");

        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMState_Constructor_Succeeds()
        {
            // Arrange & Act
            var state = new FSMState("TestState", (ctx) => { }, (ctx) => { }, (ctx) => { });

            // Assert
            Assert.That(state, Is.Not.Null);
            Assert.That(state.Name, Is.EqualTo("TestState"));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMState_Constructor_ThrowsForNullOrEmptyName()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => new FSMState(null, (ctx) => { }, (ctx) => { }, (ctx) => { }));
            Assert.Throws<ArgumentException>(() => new FSMState("", (ctx) => { }, (ctx) => { }, (ctx) => { }));
            Assert.Throws<ArgumentException>(() => new FSMState("   ", (ctx) => { }, (ctx) => { }, (ctx) => { }));
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMState_Enter_ActionIsCalled()
        {
            // Arrange
            bool entered = false;
            var state = new FSMState("EnterState", (ctx) => { entered = true; }, null, null);
            var ctx = new MockStateContext("EnterState", "TestFSM");

            // Act
            state.Enter(ctx);

            // Assert
            Assert.That(entered, Is.True, "The 'Enter' action should have been invoked.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMState_Update_ActionIsCalled()
        {
            // Arrange
            bool updated = false;
            var state = new FSMState("UpdateState", null, (ctx) => { updated = true; }, null);
            var ctx = new MockStateContext("UpdateState", "TestFSM");

            // Act
            state.Update(ctx);

            // Assert
            Assert.That(updated, Is.True, "The 'Update' action should have been invoked.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMState_Exit_ActionIsCalled()
        {
            // Arrange
            bool exited = false;
            var state = new FSMState("ExitState", null, null, (ctx) => { exited = true; });
            var ctx = new MockStateContext("ExitState", "TestFSM");

            // Act
            state.Exit(ctx);

            // Assert
            Assert.That(exited, Is.True, "The 'Exit' action should have been invoked.");
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FSMState_ActionsCanBeNull()
        {
            // Arrange, Act & Assert
            // This test ensures that providing null for the actions does not cause a crash.
            Assert.DoesNotThrow(() =>
            {
                var state = new FSMState("NullActionsState", null, null, null);
                var ctx = new MockStateContext("NullActionsState", "TestFSM");
                state.Enter(ctx);
                state.Update(ctx);
                state.Exit(ctx);
            }, "FSMState should handle null actions gracefully.");
        }

    }
}
