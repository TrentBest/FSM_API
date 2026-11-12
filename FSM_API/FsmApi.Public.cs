using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using IntegerBacked = TheSingularityWorkshop.FSM_API.IntegerBacked;

namespace FSM_API
{
    /// <summary>
    /// Stable 2.0 public facade. The legacy implementation remains available under
    /// TheSingularityWorkshop.FSM_API while consumers migrate to the shorter namespace.
    /// </summary>
    public static class FsmApi
    {
        /// <summary>Defines FSMs and creates live instances.</summary>
        public static class Create
        {
            /// <summary>Begins building a string-backed FSM definition.</summary>
            public static FSMBuilder CreateFiniteStateMachine(
                string fsmName = "UnNamedFSM",
                int processRate = 0,
                string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Create.CreateFiniteStateMachine(
                    fsmName, processRate, processingGroup);

            /// <summary>Begins building an integer-backed FSM definition.</summary>
            public static FSMBuilderInt CreateFiniteStateMachine(
                int fsmID,
                int processRate = 0,
                int processingGroupID = 0)
                => IntegerBacked.FsmApi.Create.CreateFiniteStateMachine(
                    fsmID, processRate, processingGroupID);

            /// <summary>Creates a string-backed instance bound to the caller-owned context.</summary>
            public static FSMHandle CreateInstance(
                string fsmName,
                IStateContext context,
                string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Create.CreateInstance(
                    fsmName, context, processingGroup);

            /// <summary>Creates an integer-backed instance bound to the caller-owned context.</summary>
            public static FSMHandleInt CreateInstance(int fsmID, IStateContext context)
                => IntegerBacked.FsmApi.Create.CreateInstance(fsmID, context);

            /// <summary>Registers a completed integer-backed definition.</summary>
            public static IntegerBacked.FSM RegisterDefinition(IntegerBacked.FSM definition)
                => IntegerBacked.FsmApi.Create.RegisterDefinition(definition);

            /// <summary>Ensures a string-backed processing group exists.</summary>
            public static void CreateProcessingGroup(string processingGroup)
                => TheSingularityWorkshop.FSM_API.FSM_API.Create.CreateProcessingGroup(processingGroup);
        }

        /// <summary>Runtime interaction and lifecycle operations.</summary>
        public static class Interact
        {
            /// <summary>Updates a string-backed processing group.</summary>
            public static void Update(string processingGroup)
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.Update(processingGroup);

            /// <summary>Updates an integer-backed processing group.</summary>
            public static void Update(int processingGroupID)
                => IntegerBacked.FsmApi.Interaction.Update(processingGroupID);

            /// <summary>Updates all integer-backed processing groups.</summary>
            public static void UpdateAllIntegerBacked()
                => IntegerBacked.FsmApi.Interaction.UpdateAll();

            /// <summary>Returns whether an integer-backed definition is registered.</summary>
            public static bool Contains(int fsmID)
                => IntegerBacked.FsmApi.Interaction.Contains(fsmID);

            /// <summary>Unregisters an integer-backed definition and its instances.</summary>
            public static bool Unregister(int fsmID)
                => IntegerBacked.FsmApi.Interaction.Unregister(fsmID);
        }

        /// <summary>Diagnostic notifications and error policy for the string-backed runtime.</summary>
        /// <remarks>Integer-backed error accounting is not yet part of this facade.</remarks>
        public static class Error
        {
            /// <summary>Threshold for scheduling shutdown of a repeatedly failing instance.</summary>
            public static int InstanceErrorThreshold
            {
                get => TheSingularityWorkshop.FSM_API.FSM_API.Error.InstanceErrorThreshold;
                set => TheSingularityWorkshop.FSM_API.FSM_API.Error.InstanceErrorThreshold = value;
            }

            /// <summary>Threshold for scheduling destruction of a repeatedly failing definition.</summary>
            public static int DefinitionErrorThreshold
            {
                get => TheSingularityWorkshop.FSM_API.FSM_API.Error.DefinitionErrorThreshold;
                set => TheSingularityWorkshop.FSM_API.FSM_API.Error.DefinitionErrorThreshold = value;
            }

            /// <summary>Receives internal API diagnostics.</summary>
            public static event TheSingularityWorkshop.FSM_API.FSM_API.Error.InternalApiErrorEventHandler OnInternalApiError
            {
                add => TheSingularityWorkshop.FSM_API.FSM_API.Error.OnInternalApiError += value;
                remove => TheSingularityWorkshop.FSM_API.FSM_API.Error.OnInternalApiError -= value;
            }

            /// <summary>Reports an internal diagnostic.</summary>
            public static void Report(string message, Exception exception = null)
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(message, exception);

            /// <summary>Resets accumulated errors for a string-backed definition.</summary>
            public static void ResetDefinitionErrorCount(string fsmName)
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.ResetDefinitionErrorCount(fsmName);

            /// <summary>Resets accumulated errors for a string-backed instance.</summary>
            public static void ResetInstanceErrorCount(FSMHandle handle)
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.ResetInstanceErrorCount(handle);
        }
    }
}
