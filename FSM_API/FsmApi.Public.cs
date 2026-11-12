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

        /// <summary>Explicitly converts definitions between string and integer representations.</summary>
        public static class Transfer
        {
            /// <summary>Compiles a string-backed definition into the integer-backed representation.</summary>
            public static FSMCompilationResult ToInteger(FSM definition, int fsmID, int processingGroupID = 0)
                => TheSingularityWorkshop.FSM_API.FSMCompiler.Compile(definition, fsmID, processingGroupID);

            /// <summary>Reconstructs a string-backed definition using an explicit reverse state-name map.</summary>
            public static FSM ToString(
                IntegerBacked.FSM definition,
                string fsmName,
                string processingGroup,
                IReadOnlyDictionary<int, string> stateIDToName)
                => TheSingularityWorkshop.FSM_API.FSMCompiler.Decompile(
                    definition, fsmName, processingGroup, stateIDToName);
        }

        /// <summary>Runtime interaction and lifecycle operations.</summary>
        public static class Interact
        {
            /// <summary>Updates a string-backed processing group.</summary>
            public static void Update(string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.Update(processingGroup);

            /// <summary>Updates an integer-backed processing group.</summary>
            public static void Update(int processingGroupID)
                => IntegerBacked.FsmApi.Interaction.Update(processingGroupID);

            /// <summary>Updates all integer-backed processing groups.</summary>
            public static void UpdateAllIntegerBacked()
                => IntegerBacked.FsmApi.Interaction.UpdateAll();

            /// <summary>Moves a string-backed instance to another compatible processing group.</summary>
            public static void MoveInstance(FSMHandle instance, string processingGroup)
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.MoveInstance(instance, processingGroup);

            /// <summary>Adds or replaces a state in a string-backed definition.</summary>
            public static void AddStateToFSM(
                string fsmName, string stateName,
                Action<IStateContext> onEnter, Action<IStateContext> onUpdate,
                Action<IStateContext> onExit, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.AddStateToFSM(
                    fsmName, stateName, onEnter, onUpdate, onExit, processingGroup);

            /// <summary>Adds a transition to a string-backed definition.</summary>
            public static void AddTransition(
                string fsmName, string fromState, string toState,
                Func<IStateContext, bool> condition, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.AddTransition(
                    fsmName, fromState, toState, condition, processingGroup);

            /// <summary>Destroys a string-backed definition and its instances.</summary>
            public static void DestroyFiniteStateMachine(string fsmName, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.DestroyFiniteStateMachine(fsmName, processingGroup);

            /// <summary>Destroys a string-backed instance.</summary>
            public static void DestroyInstance(FSMHandle instance)
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.DestroyInstance(instance);

            /// <summary>Returns whether a string-backed definition exists.</summary>
            public static bool Exists(string fsmName, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.Exists(fsmName, processingGroup);

            /// <summary>Returns all string-backed definition names in a processing group.</summary>
            public static IReadOnlyCollection<string> GetAllDefinitionNames(string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.GetAllDefinitionNames(processingGroup);

            /// <summary>Returns all live instances of a string-backed definition.</summary>
            public static IReadOnlyList<FSMHandle> GetInstances(string fsmName, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.GetInstances(fsmName, processingGroup);

            /// <summary>Returns a string-backed instance for the supplied context.</summary>
            public static FSMHandle GetInstance(string fsmName, IStateContext context, string processingGroup)
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.GetInstance(fsmName, context, processingGroup);

            /// <summary>Returns a string-backed FSM definition.</summary>
            public static FSM GetFSMDefinition(string fsmName, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.GetFSMDefinition(fsmName, processingGroup);

            /// <summary>Removes a processing group.</summary>
            public static void RemoveProcessingGroup(string processingGroup)
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.RemoveProcessingGroup(processingGroup);

            /// <summary>Removes a state and redirects affected instances to the specified fallback state.</summary>
            public static void RemoveStateFromFSM(
                string fsmName, string stateName, string fallbackStateName, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.RemoveStateFromFSM(
                    fsmName, stateName, fallbackStateName, processingGroup);

            /// <summary>Removes a transition from a string-backed definition.</summary>
            public static void RemoveTransition(
                string fsmName, string fromState, string toState, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Interaction.RemoveTransition(
                    fsmName, fromState, toState, processingGroup);

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

            /// <summary>Records an error for a string-backed live instance.</summary>
            public static void InvokeInstanceError(
                FSMHandle handle, string message, Exception exception, string processingGroup = "Update")
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInstanceError(
                    handle, message, exception, processingGroup);

            /// <summary>Records an error for a string-backed definition.</summary>
            public static void InvokeDefinitionError(string fsmName, string processingGroup)
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeDefinitionError(fsmName, processingGroup);

            /// <summary>Resets accumulated errors for a string-backed instance.</summary>
            public static void ResetInstanceErrorCount(FSMHandle handle)
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.ResetInstanceErrorCount(handle);

            /// <summary>Returns the current string-backed instance error counts.</summary>
            public static Dictionary<FSMHandle, int> GetErrorCounts()
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.GetErrorCounts();

            /// <summary>Returns the current string-backed definition error counts.</summary>
            public static Dictionary<string, int> GetDefinitionErrorCounts()
                => TheSingularityWorkshop.FSM_API.FSM_API.Error.GetDefinitionErrorCounts();
        }
    }
}
