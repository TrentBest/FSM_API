using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API.Tests.Internal;

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
        public void State_With_Invalid_Name_Throws_Exception()
        {
            Assert.Throws<ArgumentException>(() => new FSMState("", null, null, null), "State with empty name should throw exception");
            Assert.Throws<ArgumentException>(() => new FSMState("   ", null, null, null), "State with whitespace name should throw exception");
        }

        /// <summary>
        /// Tests that the OnEnter action is correctly set and invoked.
        /// </summary>
        [Test]
        public void SetOnEnter_ActionIsInvoked()
        {
            bool entered = false;
            Action<IStateContext> onEnter = (ctx) => entered = true;
            FSMState state = new FSMState("StateWithOnEnter", null, null, null);

            state.SetOnEnter(onEnter);
            state.Enter(new FSMTestContext());

            Assert.That(entered, Is.True, "OnEnter action should have been invoked.");
        }

        /// <summary>
        /// Tests that the OnUpdate action is correctly set and invoked.
        /// </summary>
        [Test]
        public void SetOnUpdate_ActionIsInvoked()
        {
            bool updated = false;
            Action<IStateContext> onUpdate = (ctx) => updated = true;
            FSMState state = new FSMState("StateWithOnUpdate", null, null, null);

            state.SetOnUpdate(onUpdate);
            state.Update(new FSMTestContext());

            Assert.That(updated, Is.True, "OnUpdate action should have been invoked.");
        }

        /// <summary>
        /// Tests that the OnExit action is correctly set and invoked.
        /// </summary>
        [Test]
        public void SetOnExit_ActionIsInvoked()
        {
            bool exited = false;
            Action<IStateContext> onExit = (ctx) => exited = true;
            FSMState state = new FSMState("StateWithOnExit", null, null, null);

            state.SetOnExit(onExit);
            state.Exit(new FSMTestContext());

            Assert.That(exited, Is.True, "OnExit action should have been invoked.");
        }

        /// <summary>
        /// Tests that if SetOnEnter is called with null, the default no-op action is used.
        /// </summary>
        [Test]
        public void SetOnEnter_WithNull_DoesNotThrow()
        {
            FSMState state = new FSMState("StateNullOnEnter", (ctx) => { throw new Exception("Should not run"); }, null, null);
            Assert.DoesNotThrow(() => state.SetOnEnter(null)); // Assign null
            Assert.DoesNotThrow(() => state.Enter(new FSMTestContext())); // Invoke after null assignment
        }

        /// <summary>
        /// Tests that if SetOnUpdate is called with null, the default no-op action is used.
        /// </summary>
        [Test]
        public void SetOnUpdate_WithNull_DoesNotThrow()
        {
            FSMState state = new FSMState("StateNullOnUpdate", null, (ctx) => { throw new Exception("Should not run"); }, null);
            Assert.DoesNotThrow(() => state.SetOnUpdate(null)); // Assign null
            Assert.DoesNotThrow(() => state.Update(new FSMTestContext())); // Invoke after null assignment
        }

        /// <summary>
        /// Tests that if SetOnExit is called with null, the default no-op action is used.
        /// </summary>
        [Test]
        public void SetOnExit_WithNull_DoesNotThrow()
        {
            FSMState state = new FSMState("StateNullOnExit", null, null, (ctx) => { throw new Exception("Should not run"); });
            Assert.DoesNotThrow(() => state.SetOnExit(null)); // Assign null
            Assert.DoesNotThrow(() => state.Exit(new FSMTestContext())); // Invoke after null assignment
        }

        /// <summary>
        /// Tests the ToString method returns the expected format.
        /// </summary>
        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            string stateName = "MyAwesomeState";
            FSMState state = new FSMState(stateName, null, null, null);

            string expected = $"FSMState: {stateName}";
            Assert.That(state.ToString(), Is.EqualTo(expected), "ToString should return 'FSMState: [StateName]'.");
        }
    }
}
