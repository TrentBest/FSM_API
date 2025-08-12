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
    public class FSM_API_Internal_ProcessDeferredModifications_Tests
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
        public void ProcessDeferredModifications_Empty()
        {
            // Arrange
            FSM_API.Internal.ResetAPI(true);
            // Act
            FSM_API.Internal.ProcessDeferredModifications();
            // Assert
            Assert.Pass("No exceptions thrown for empty deferred modifications.");
        }

        

    }
}
