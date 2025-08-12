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
    public class FSM_API_Internal_GetProcessingGroups_Tests
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
        public void GetProcessingGroups_Empty_Test()
        {
           
        
            var groups = FSM_API.Internal.GetProcessingGroups();
        
            Assert.That(groups,Is.Not.Null, "GetProcessingGroups should not return null.");
            Assert.That(groups, Is.Empty, "GetProcessingGroups should return an empty list when no groups are defined.");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetProcessingGroups_WithManyProcessingGroups_Test()
        {
            string groupName = "ProcessingGroup";
            for (int i = 0; i < 100; i++)
            {
                FSM_API.Create.CreateProcessingGroup($"{groupName}_{i}");
            }
            var groups = FSM_API.Internal.GetProcessingGroups();

            Assert.That(groups, Is.Not.Null, "GetProcessingGroups should not return null.");
            Assert.That(groups.Count, Is.EqualTo(100), "GetProcessingGroups should return an empty list when the group name is invalid.");
        }
    }
}
