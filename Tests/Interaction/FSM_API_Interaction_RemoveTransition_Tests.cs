using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using TheSingularityWorkshop.FSM_API;

using static TheSingularityWorkshop.FSM_API.FsmApi.Internal;
using TheSingularityWorkshop.FSM_API.Tests;


namespace TheSingularityWorkshop.FSM_API.Tests.Interaction
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FSM_API_Interaction_RemoveTransition_Tests
    {
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            FsmApi.Internal.ResetAPI(true);
        }
        [Test]
        public void RemoveTransition_WithAnyStateIdentifier_RemovesAnyStateTransition()
        {
            const string fsmName = "RemoveAnyStateTransitionFSM";
            const string processingGroup = "Update";

            FsmApi.Create.CreateFiniteStateMachine(fsmName, -1, processingGroup)
                .State("Idle", null, null, null)
                .State("Finished", null, null, null)
                .BuildDefinition();

            FsmApi.Interaction.AddTransition(
                fsmName,
                FSM.AnyStateIdentifier,
                "Finished",
                _ => true,
                processingGroup);

            var definition = FsmApi.Interaction.GetFSMDefinition(fsmName, processingGroup);
            Assert.That(definition.HasTransition(FSM.AnyStateIdentifier, "Finished"), Is.True);

            Assert.DoesNotThrow(() => FsmApi.Interaction.RemoveTransition(
                fsmName,
                FSM.AnyStateIdentifier,
                "Finished",
                processingGroup));

            Assert.That(definition.HasTransition(FSM.AnyStateIdentifier, "Finished"), Is.False);
            Assert.That(definition.GetAllTransitions(), Is.Empty);
        }

    }
}
