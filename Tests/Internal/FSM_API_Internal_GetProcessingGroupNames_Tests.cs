using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FSM_API.Internal;
using TheSingularityWorkshop.FSM_API.Tests;


namespace TheSingularityWorkshop.FSM_API.Tests.Internal
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Internal_GetProcessingGroupNames_Tests
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
        public void GetProcessingGroupNames_Empty_Test()
        {
            Assert.That(FSM_API.Internal.GetProcessingGroupNames(), Is.Not.Null, "GetProcessingGroupNames should not return null.");
            Assert.That(FSM_API.Internal.GetProcessingGroupNames(), Is.Empty, "GetProcessingGroup");
        }

        /// <summary>
        /// Tests that GetProcessingGroupNames returns an empty list when no groups exist.
        /// </summary>
        [Test]
        public void GetProcessingGroupNames_ReturnsEmptyListWhenNoGroupsExist()
        {
            // Act
            var names = FSM_API.Internal.GetProcessingGroupNames();

            // Assert
            Assert.That(names, Is.Empty);
        }

        /// <summary>
        /// Tests that GetProcessingGroupNames returns the correct names when groups exist.
        /// </summary>
        [Test]
        public void GetProcessingGroupNames_ReturnsCorrectNamesForExistingGroups()
        {
            // Arrange
            FSM_API.Create.CreateProcessingGroup("GroupA");
            FSM_API.Create.CreateProcessingGroup("GroupB");

            // Act
            var names = FSM_API.Internal.GetProcessingGroupNames();

            // Assert
            Assert.That(names.Count(), Is.EqualTo(2));
            Assert.That(names.OrderBy(n => n), Is.EqualTo(new[] { "GroupA", "GroupB" }));
        }
    }
}
