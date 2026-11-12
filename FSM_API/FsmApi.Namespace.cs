using System;
using System.Collections.Generic;
using Legacy = TheSingularityWorkshop.FSM_API;
using LegacyApi = TheSingularityWorkshop.FSM_API.FSM_API;
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
            public static Legacy.FSMBuilder CreateFiniteStateMachine(
                string fsmName = "UnNamedFSM",
                int processRate = 0,
                string processingGroup = "Update")
            {
                return UnifiedApi.Create.CreateFiniteStateMachine(fsmName, processRate, processingGroup);
            }

            public static Integer.FSMBuilderInt CreateFiniteStateMachine(
                int fsmID,
                int processRate = 0,
                int processingGroupID = 0)
            {
                return UnifiedApi.Create.CreateFiniteStateMachine(fsmID, processRate, processingGroupID);
            }

            public static Legacy.FSMHandle CreateInstance(
                string fsmName,
                Legacy.IStateContext context,
                string processingGroup = "Update")
            {
                return UnifiedApi.Create.CreateInstance(fsmName, context, processingGroup);
            }

            public static Integer.FSMHandleInt CreateInstance(int fsmID, Legacy.IStateContext context)
            {
                return UnifiedApi.Create.CreateInstance(fsmID, context);
            }

            public static Integer.FSM RegisterDefinition(Integer.FSM definition)
            {
                return UnifiedApi.Create.RegisterDefinition(definition);
            }

            public static void CreateProcessingGroup(string processingGroup)
            {
                UnifiedApi.Create.CreateProcessingGroup(processingGroup);
            }
        }

        /// <summary>Explicit conversion between string and integer-backed definitions.</summary>
        public static class Transfer
        {
            public static Legacy.FSMCompilationResult ToInteger(
                Legacy.FSM definition,
                int fsmID,
                int processingGroupID = 0)
            {
                return UnifiedApi.Transfer.ToInteger(definition, fsmID, processingGroupID);
            }

            public static Legacy.FSM ToString(
                Integer.FSM definition,
                string fsmName,
                string processingGroup,
                IReadOnlyDictionary<int, string> stateIDToName)
            {
                return UnifiedApi.Transfer.ToString(
                    definition, fsmName, processingGroup, stateIDToName);
            }
        }

        /// <summary>Scheduled updates, inspection, mutation, and explicit lifecycle operations.</summary>
        public static class Interact
        {
            public static void Update(string processingGroup = "Update")
            {
                LegacyApi.Interaction.Update(processingGroup);
            }

            public static void Update(int processingGroupID)
            {
                Integer.FsmApi.Interaction.Update(processingGroupID);
            }

            public static void UpdateAllIntegerBacked()
            {
                Integer.FsmApi.Interaction.UpdateAll();
            }

            public static bool Unregister(int fsmID)
            {
                return Integer.FsmApi.Interaction.Unregister(fsmID);
            }

            public static bool Contains(int fsmID)
            {
                return Integer.FsmApi.Interaction.Contains(fsmID);
            }

            public static void MoveInstance(Legacy.FSMHandle instance, string processingGroup)
            {
                LegacyApi.Interaction.MoveInstance(instance, processingGroup);
            }

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

            public static void DestroyFiniteStateMachine(
                string fsmName,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.DestroyFiniteStateMachine(fsmName, processingGroup);
            }

            public static bool Exists(string fsmName, string processingGroup = "Update")
            {
                return LegacyApi.Interaction.Exists(fsmName, processingGroup);
            }

            public static IReadOnlyCollection<string> GetAllDefinitionNames(
                string processingGroup = "Update")
            {
                return LegacyApi.Interaction.GetAllDefinitionNames(processingGroup);
            }

            public static IReadOnlyList<Legacy.FSMHandle> GetInstances(
                string fsmName,
                string processingGroup = "Update")
            {
                return LegacyApi.Interaction.GetInstances(fsmName, processingGroup);
            }

            public static void RemoveProcessingGroup(string processingGroup)
            {
                LegacyApi.Interaction.RemoveProcessingGroup(processingGroup);
            }

            public static void DestroyInstance(Legacy.FSMHandle instance)
            {
                LegacyApi.Interaction.DestroyInstance(instance);
            }

            public static void RemoveStateFromFSM(
                string fsmName,
                string stateName,
                string fallbackStateName,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.RemoveStateFromFSM(
                    fsmName, stateName, fallbackStateName, processingGroup);
            }

            public static void RemoveTransition(
                string fsmName,
                string fromState,
                string toState,
                string processingGroup = "Update")
            {
                LegacyApi.Interaction.RemoveTransition(
                    fsmName, fromState, toState, processingGroup);
            }

            public static Legacy.FSMHandle GetInstance(
                string fsmName,
                Legacy.IStateContext context,
                string processingGroup)
            {
                return LegacyApi.Interaction.GetInstance(fsmName, context, processingGroup);
            }

            public static Legacy.FSM GetFSMDefinition(
                string fsmName,
                string processingGroup = "Update")
            {
                return LegacyApi.Interaction.GetFSMDefinition(fsmName, processingGroup);
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

            public static void InvokeInternalApiError(string message, Exception exception)
            {
                LegacyApi.Error.InvokeInternalApiError(message, exception);
            }

            public static void InvokeInstanceError(
                Legacy.FSMHandle handle,
                string message,
                Exception exception,
                string processGroup = "Update")
            {
                LegacyApi.Error.InvokeInstanceError(handle, message, exception, processGroup);
            }

            public static void InvokeDefinitionError(string fsmDefinitionName, string processingGroup)
            {
                LegacyApi.Error.InvokeDefinitionError(fsmDefinitionName, processingGroup);
            }

            public static void ResetDefinitionErrorCount(string fsmDefinitionName)
            {
                LegacyApi.Error.ResetDefinitionErrorCount(fsmDefinitionName);
            }

            public static void ResetInstanceErrorCount(Legacy.FSMHandle handle)
            {
                LegacyApi.Error.ResetInstanceErrorCount(handle);
            }

            public static Dictionary<Legacy.FSMHandle, int> GetErrorCounts()
            {
                return LegacyApi.Error.GetErrorCounts();
            }

            public static Dictionary<string, int> GetDefinitionErrorCounts()
            {
                return LegacyApi.Error.GetDefinitionErrorCounts();
            }

            public static void Reset()
            {
                LegacyApi.Error.Reset();
            }
        }
    }
}
