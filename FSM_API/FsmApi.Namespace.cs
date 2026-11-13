using System;
using System.Collections.Generic;
using Legacy = TheSingularityWorkshop.FSM_API;
using LegacyApi = TheSingularityWorkshop.FSM_API.FsmApi;
using UnifiedApi = TheSingularityWorkshop.FSM_API.FsmApi;
using Integer = TheSingularityWorkshop.FSM_API.IntegerBacked;

namespace FSM_API
{
    /// <summary>
    /// The concise 2.0 consumer namespace facade. It exposes Create, Interact, and Error
    /// while forwarding to the existing string-backed runtime and the integer-backed runtime.
    /// Returned runtime types remain in the established TheSingularityWorkshop.FSM_API
    /// namespaces during this additive compatibility phase.
    /// </summary>
    public static class FsmApi
    {
        /// <summary>Definition and live-instance construction.</summary>
        public static class Create
        {
            /// <summary>Creates a builder for a named or integer-identified FSM definition.</summary>
            public static Legacy.FSMBuilder CreateFiniteStateMachine(
                string fsmName = "UnNamedFSM",
                int processRate = 0,
                string processingGroup = "Update")
            {
                return UnifiedApi.Create.CreateFiniteStateMachine(fsmName, processRate, processingGroup);
            }

            /// <summary>Creates a builder for a named or integer-identified FSM definition.</summary>
            public static Integer.FSMBuilderInt CreateFiniteStateMachine(
                int fsmID,
                int processRate = 0,
                int processingGroupID = 0)
            {
                return UnifiedApi.Create.CreateFiniteStateMachine(fsmID, processRate, processingGroupID);
            }

            /// <summary>Creates a live FSM instance bound to a caller-owned context.</summary>
            public static Legacy.FSMHandle CreateInstance(
                string fsmName,
                Legacy.IStateContext context,
                string processingGroup = "Update")
            {
                return UnifiedApi.Create.CreateInstance(fsmName, context, processingGroup);
            }

            /// <summary>Creates a live FSM instance bound to a caller-owned context.</summary>
            public static Integer.FSMHandleInt CreateInstance(int fsmID, Legacy.IStateContext context)
            {
                return UnifiedApi.Create.CreateInstance(fsmID, context);
            }

            /// <summary>Registers a completed integer-backed FSM definition.</summary>
            public static Integer.FSM RegisterDefinition(Integer.FSM definition)
            {
                return UnifiedApi.Create.RegisterDefinition(definition);
            }

            /// <summary>Ensures a named string-backed processing group exists.</summary>
            public static void CreateProcessingGroup(string processingGroup)
            {
                UnifiedApi.Create.CreateProcessingGroup(processingGroup);
            }

        }

        /// <summary>Explicit conversion between string-backed and integer-backed definitions.</summary>
        public static class Transfer
        {
            /// <summary>Compiles a string-backed definition into the integer-backed representation.</summary>
            public static Integer.FSMCompilationResult ToInteger(Legacy.FSM definition, int fsmID, int processingGroupID = 0)
            {
                return UnifiedApi.Transfer.ToInteger(definition, fsmID, processingGroupID);
            }

            /// <summary>Reconstructs a string-backed definition using the supplied state-name mapping.</summary>
            public static Legacy.FSM ToString(Integer.FSM definition, string fsmName, string processingGroup, IReadOnlyDictionary<int, string> stateIDToName)
            {
                return UnifiedApi.Transfer.ToString(definition, fsmName, processingGroup, stateIDToName);
            }
        }

        /// <summary>Scheduled updates, inspection, mutation, and explicit lifecycle operations.</summary>
        public static class Interact
        {
            /// <summary>Advances the selected processing group by one update step.</summary>
            public static void Update(string processingGroup = "Update")
            {
                LegacyApi.Interaction.Update(processingGroup);
            }

            /// <summary>Advances the selected processing group by one update step.</summary>
            public static void Update(int processingGroupID)
            {
                Integer.FsmApi.Interaction.Update(processingGroupID);
            }

            /// <summary>Advances every eligible integer-backed processing group.</summary>
            public static void UpdateAllIntegerBacked()
            {
                Integer.FsmApi.Interaction.UpdateAll();
            }

            /// <summary>Removes an integer-backed definition and its instances.</summary>
            public static bool Unregister(int fsmID)
            {
                return Integer.FsmApi.Interaction.Unregister(fsmID);
            }

            /// <summary>Checks whether an integer-backed definition is registered.</summary>
            public static bool Contains(int fsmID)
            {
                return Integer.FsmApi.Interaction.Contains(fsmID);
            }

            /// <summary>Moves a string-backed instance to another processing group.</summary>
            public static void MoveInstance(Legacy.FSMHandle instance, string processingGroup)
            {
                LegacyApi.Interaction.MoveInstance(instance, processingGroup);
            }

            /// <summary>Adds a state to an existing string-backed definition.</summary>
            public static void AddStateToFSM(
                string fsmName,
                string stateName,
                Action<Legacy.IStateContext> onEnter,
                Action<Legacy.IStateContext> onUpdate,
                Action<Legacy.IStateContext> onExit,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.AddStateToFSM(
                    fsmName, stateName, onEnter, onUpdate, onExit, processingGroup);
            }

            /// <summary>Adds a transition to an existing string-backed definition.</summary>
            public static void AddTransition(
                string fsmName,
                string fromState,
                string toState,
                Func<Legacy.IStateContext, bool> condition,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.AddTransition(
                    fsmName, fromState, toState, condition, processingGroup);
            }

            /// <summary>Destroys a named definition and its registered instances.</summary>
            public static void DestroyFiniteStateMachine(
                string fsmName,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.DestroyFiniteStateMachine(fsmName, processingGroup);
            }

            /// <summary>Checks whether a named definition exists in a processing group.</summary>
            public static bool Exists(string fsmName, string processingGroup = "Update")
            {
                return LegacyApi.Interaction.Exists(fsmName, processingGroup);
            }

            /// <summary>Returns the definition names registered in a processing group.</summary>
            public static IReadOnlyCollection<string> GetAllDefinitionNames(
                string processingGroup = "Update")
            {
                return LegacyApi.Interaction.GetAllDefinitionNames(processingGroup);
            }

            /// <summary>Returns the live instances associated with a definition.</summary>
            public static IReadOnlyList<Legacy.FSMHandle> GetInstances(
                string fsmName,
                string processingGroup = "Update")
            {
                return LegacyApi.Interaction.GetInstances(fsmName, processingGroup);
            }

            /// <summary>Destroys a processing group and its registered definitions and instances.</summary>
            public static void RemoveProcessingGroup(string processingGroup)
            {
                LegacyApi.Interaction.RemoveProcessingGroup(processingGroup);
            }

            /// <summary>Explicitly destroys a live FSM instance.</summary>
            public static void DestroyInstance(Legacy.FSMHandle instance)
            {
                LegacyApi.Interaction.DestroyInstance(instance);
            }

            /// <summary>Removes a state from an existing definition.</summary>
            public static void RemoveStateFromFSM(
                string fsmName,
                string stateName,
                string fallbackStateName,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.RemoveStateFromFSM(
                    fsmName, stateName, fallbackStateName, processingGroup);
            }

            /// <summary>Removes a transition from an existing definition.</summary>
            public static void RemoveTransition(
                string fsmName,
                string fromState,
                string toState,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.RemoveTransition(
                    fsmName, fromState, toState, processingGroup);
            }

            /// <summary>Finds a live instance by definition, context, and processing group.</summary>
            public static Legacy.FSMHandle GetInstance(
                string fsmName,
                Legacy.IStateContext context,
                string processingGroup)
            {
                return LegacyApi.Interaction.GetInstance(fsmName, context, processingGroup);
            }

            /// <summary>Returns a named or integer-identified FSM definition.</summary>
            public static Legacy.FSM GetFSMDefinition(
                string fsmName,
                string processingGroup = "Update")
            {
                return LegacyApi.Interaction.GetFSMDefinition(fsmName, processingGroup);
            }

            /// <summary>Returns a named or integer-identified FSM definition.</summary>
            public static Integer.FSM GetFSMDefinition(int fsmID)
            {
                return Integer.FsmApi.Interaction.GetDefinition(fsmID);
            }

            /// <summary>Returns the live instances associated with a definition.</summary>
            public static IReadOnlyCollection<Integer.FSMHandleInt> GetInstances(int fsmID)
            {
                return Integer.FsmApi.Runtime.GetInstances(fsmID);
            }

            /// <summary>Explicitly destroys a live FSM instance.</summary>
            public static bool DestroyInstance(Integer.FSMHandleInt instance)
            {
                return Integer.FsmApi.Interaction.DestroyInstance(instance);
            }
        }

        /// <summary>Diagnostic notifications and error-policy controls for the string-backed runtime.</summary>
        public static class Error
        {
            public static int InstanceErrorThreshold
            {
                get => LegacyApi.Error.InstanceErrorThreshold;
                set => LegacyApi.Error.InstanceErrorThreshold = value;
            }

            public static int DefinitionErrorThreshold
            {
                get => LegacyApi.Error.DefinitionErrorThreshold;
                set => LegacyApi.Error.DefinitionErrorThreshold = value;
            }

            public static event LegacyApi.Error.InternalApiErrorEventHandler OnInternalApiError
            {
                add => LegacyApi.Error.OnInternalApiError += value;
                remove => LegacyApi.Error.OnInternalApiError -= value;
            }

            /// <summary>Raises a diagnostic notification.</summary>
            public static void InvokeInternalApiError(string message, Exception exception)
            {
                LegacyApi.Error.InvokeInternalApiError(message, exception);
            }

            /// <summary>Reports a diagnostic message through the shared error channel.</summary>
            public static void Report(string message, Exception exception = null)
            {
                LegacyApi.Error.InvokeInternalApiError(message, exception);
            }

            /// <summary>Records an error for a specific string-backed instance.</summary>
            public static void InvokeInstanceError(
                Legacy.FSMHandle handle,
                string message,
                Exception exception,
                string processGroup = "Update")
            {
                LegacyApi.Error.InvokeInstanceError(handle, message, exception, processGroup);
            }

            /// <summary>Records an error for a named definition.</summary>
            public static void InvokeDefinitionError(string fsmDefinitionName, string processingGroup)
            {
                LegacyApi.Error.InvokeDefinitionError(fsmDefinitionName, processingGroup);
            }

            /// <summary>Resets the error count for a named definition.</summary>
            public static void ResetDefinitionErrorCount(string fsmDefinitionName)
            {
                LegacyApi.Error.ResetDefinitionErrorCount(fsmDefinitionName);
            }

            /// <summary>Resets the error count for a specific instance.</summary>
            public static void ResetInstanceErrorCount(Legacy.FSMHandle handle)
            {
                LegacyApi.Error.ResetInstanceErrorCount(handle);
            }

            /// <summary>Returns a snapshot of string-backed instance error counts.</summary>
            public static Dictionary<Legacy.FSMHandle, int> GetErrorCounts()
            {
                return LegacyApi.Error.GetErrorCounts();
            }

            /// <summary>Returns a snapshot of string-backed definition error counts.</summary>
            public static Dictionary<string, int> GetDefinitionErrorCounts()
            {
                return LegacyApi.Error.GetDefinitionErrorCounts();
            }

            /// <summary>Resets string-backed diagnostic counters.</summary>
            public static void Reset()
            {
                LegacyApi.Error.Reset();
            }

            /// <summary>Returns a snapshot of integer-backed instance error counts.</summary>
            public static Dictionary<Integer.FSMHandleInt, int> GetIntegerInstanceErrorCounts()
            {
                return Integer.FsmApi.Runtime.GetInstanceErrorCounts();
            }

            /// <summary>Returns a snapshot of integer-backed definition error counts.</summary>
            public static Dictionary<int, int> GetIntegerDefinitionErrorCounts()
            {
                return Integer.FsmApi.Runtime.GetDefinitionErrorCounts();
            }

            /// <summary>Resets the error count for an integer-backed instance.</summary>
            public static void ResetIntegerInstanceErrorCount(Integer.FSMHandleInt handle)
            {
                Integer.FsmApi.Runtime.ResetInstanceErrorCount(handle);
            }

            /// <summary>Resets the error count for an integer-backed definition.</summary>
            public static void ResetIntegerDefinitionErrorCount(int fsmID)
            {
                Integer.FsmApi.Runtime.ResetDefinitionErrorCount(fsmID);
            }
        }
    }
}
