using TheSingularityWorkshop.FSM_API.IntegerBacked;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Unified public entry point for string-backed and integer-backed FSM operations.
    /// </summary>
    /// <remarks>
    /// The overload selected by the identity type determines the representation used at runtime:
    /// strings use the legacy named runtime, while integers use the direct-indexed integer runtime.
    /// The two representations do not implicitly translate identities or share registries.
    /// </remarks>
    public static class FsmApi
    {
        /// <summary>Creates definitions and live instances in either supported representation.</summary>
        public static class Create
        {
            /// <summary>Begins building a string-backed FSM definition.</summary>
            public static FSMBuilder CreateFiniteStateMachine(
                string fsmName = "UnNamedFSM",
                int processRate = 0,
                string processingGroup = "Update")
            {
                return FSM_API.Create.CreateFiniteStateMachine(fsmName, processRate, processingGroup);
            }

            /// <summary>Begins building an integer-backed FSM definition.</summary>
            public static FSMBuilderInt CreateFiniteStateMachine(
                int fsmID,
                int processRate = 0,
                int processingGroupID = 0)
            {
                return IntegerBacked.FsmApi.Create.CreateFiniteStateMachine(fsmID, processRate, processingGroupID);
            }

            /// <summary>Creates an instance of a string-backed FSM definition.</summary>
            public static FSMHandle CreateInstance(
                string fsmName,
                IStateContext context,
                string processingGroup = "Update")
            {
                return FSM_API.Create.CreateInstance(fsmName, context, processingGroup);
            }

            /// <summary>Registers a completed integer-backed definition.</summary>
            public static IntegerBacked.FSM RegisterDefinition(IntegerBacked.FSM definition)
            {
                return IntegerBacked.FsmApi.Create.RegisterDefinition(definition);
            }

            /// <summary>Creates an instance of an integer-backed FSM definition.</summary>
            public static FSMHandleInt CreateInstance(int fsmID, IStateContext context)
            {
                return IntegerBacked.FsmApi.Create.CreateInstance(fsmID, context);
            }

            /// <summary>Ensures a named string-backed processing group exists.</summary>
            public static void CreateProcessingGroup(string processingGroup)
            {
                FSM_API.Create.CreateProcessingGroup(processingGroup);
            }
        }

        /// <summary>Updates FSMs using the identity type's corresponding runtime.</summary>
        public static class Interaction
        {
            /// <summary>Updates the named string-backed processing group.</summary>
            public static void Update(string processingGroup)
            {
                FSM_API.Interaction.Update(processingGroup);
            }

            /// <summary>Updates the integer-backed processing group.</summary>
            public static void Update(int processingGroupID)
            {
                IntegerBacked.FsmApi.Interaction.Update(processingGroupID);
            }

            /// <summary>Updates all integer-backed processing groups.</summary>
            public static void UpdateAllIntegerBacked()
            {
                IntegerBacked.FsmApi.Interaction.UpdateAll();
            }

            /// <summary>Removes an integer-backed definition and its instances.</summary>
            public static bool Unregister(int fsmID)
            {
                return IntegerBacked.FsmApi.Interaction.Unregister(fsmID);
            }

            /// <summary>Returns whether an integer-backed definition is registered.</summary>
            public static bool Contains(int fsmID)
            {
                return IntegerBacked.FsmApi.Interaction.Contains(fsmID);
            }
        }
    }
}
