using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace TheSingularityWorkshop.FSM_API.Tests
{
    /// <summary>
    /// This test fixture contains all the unit tests for the FSMTransition class.
    /// It ensures that the transition rules are created and behave as expected.
    /// </summary>
    [TestFixture]
    public class FSM_API_FSMTransition_Tests
    {
        /// <summary>
        /// This setup method runs before each test.
        /// It's a good practice to reset the API to a known state for reliable testing.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FSM_API.Internal.ResetAPI(true);
        }

        /// <summary>
        /// This test checks if a valid FSMTransition can be created successfully.
        /// It verifies that the properties are correctly assigned by the constructor.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithValidArguments_Succeeds()
        {
            // Arrange
            string fromState = "Idle";
            string toState = "Running";
            Func<IStateContext, bool> condition = (context) => true;

            // Act
            var transition = new FSMTransition(fromState, toState, condition);

            // Assert
            Assert.That(transition.From, Is.EqualTo(fromState));
            Assert.That(transition.To, Is.EqualTo(toState));
            Assert.That(transition.Condition, Is.EqualTo(condition));
        }

        /// <summary>
        /// This test verifies that the constructor throws an ArgumentException
        /// when the 'from' state name is null.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithNullFromState_ThrowsArgumentException()
        {
            // Arrange
            string fromState = null;
            string toState = "Running";
            Func<IStateContext, bool> condition = (context) => true;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new FSMTransition(fromState, toState, condition));
        }

        /// <summary>
        /// This test verifies that the constructor throws an ArgumentException
        /// when the 'from' state name is an empty string.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithEmptyFromState_ThrowsArgumentException()
        {
            // Arrange
            string fromState = string.Empty;
            string toState = "Running";
            Func<IStateContext, bool> condition = (context) => true;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new FSMTransition(fromState, toState, condition));
        }

        /// <summary>
        /// This test verifies that the constructor throws an ArgumentException
        /// when the 'from' state name is only whitespace.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithWhitespaceFromState_ThrowsArgumentException()
        {
            // Arrange
            string fromState = "   ";
            string toState = "Running";
            Func<IStateContext, bool> condition = (context) => true;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new FSMTransition(fromState, toState, condition));
        }

        /// <summary>
        /// This test verifies that the constructor throws an ArgumentException
        /// when the 'to' state name is null.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithNullToState_ThrowsArgumentException()
        {
            // Arrange
            string fromState = "Idle";
            string toState = null;
            Func<IStateContext, bool> condition = (context) => true;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new FSMTransition(fromState, toState, condition));
        }

        /// <summary>
        /// This test verifies that the constructor throws an ArgumentException
        /// when the 'to' state name is an empty string.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithEmptyToState_ThrowsArgumentException()
        {
            // Arrange
            string fromState = "Idle";
            string toState = string.Empty;
            Func<IStateContext, bool> condition = (context) => true;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new FSMTransition(fromState, toState, condition));
        }

        /// <summary>
        /// This test verifies that the constructor throws an ArgumentException
        /// when the 'to' state name is only whitespace.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithWhitespaceToState_ThrowsArgumentException()
        {
            // Arrange
            string fromState = "Idle";
            string toState = "   ";
            Func<IStateContext, bool> condition = (context) => true;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new FSMTransition(fromState, toState, condition));
        }

        /// <summary>
        /// This test verifies that the constructor throws an ArgumentNullException
        /// when the condition is null.
        /// </summary>
        [Test]
        public void FSMTransition_Constructor_WithNullCondition_ThrowsArgumentNullException()
        {
            // Arrange
            string fromState = "Idle";
            string toState = "Running";
            Func<IStateContext, bool> condition = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FSMTransition(fromState, toState, condition));
        }

        /// <summary>
        /// This test checks that the ToString() method returns the expected
        /// string format.
        /// </summary>
        [Test]
        public void FSMTransition_ToString_ReturnsCorrectFormat()
        {
            // Arrange
            string fromState = "Idle";
            string toState = "Running";
            Func<IStateContext, bool> condition = (context) => true;
            var transition = new FSMTransition(fromState, toState, condition);
            string expectedString = "Idle --[Condition]--> Running";

            // Act
            string actualString = transition.ToString();

            // Assert
            Assert.That(actualString, Is.EqualTo(expectedString));
        }
    }
}
