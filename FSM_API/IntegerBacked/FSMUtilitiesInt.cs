namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Validation and inspection helpers for integer-backed FSM identities and configuration values.
    /// </summary>
    public static class FSMUtilitiesInt
    {
        /// <summary>Returns <c>true</c> when the value is a valid non-negative FSM identity.</summary>
        public static bool IsValidFSMID(int fsmID)
        {
            return fsmID >= 0;
        }

        /// <summary>Returns <c>true</c> when the value is a valid non-negative state identity.</summary>
        public static bool IsValidStateID(int stateID)
        {
            return stateID >= 0 && stateID != FSM.AnyStateIdentifier;
        }

        /// <summary>Returns <c>true</c> when the value is the reserved Any-State identity.</summary>
        public static bool IsAnyStateIdentifier(int stateID)
        {
            return stateID == FSM.AnyStateIdentifier;
        }

        /// <summary>Returns <c>true</c> when the value is a valid non-negative processing-group identity.</summary>
        public static bool IsValidProcessingGroupID(int processingGroupID)
        {
            return processingGroupID >= 0;
        }

        /// <summary>Returns <c>true</c> when a process rate uses one of the supported semantics.</summary>
        /// <remarks><c>-1</c> means every tick, <c>0</c> means manual, and positive values mean every Nth tick.</remarks>
        public static bool IsValidProcessRate(int processRate)
        {
            return processRate >= -1;
        }

        /// <summary>Attempts to retrieve a state without throwing for an unknown identity.</summary>
        public static bool TryGetState(FSM definition, int stateID, out FSMState state)
        {
            if (definition == null)
            {
                state = null;
                return false;
            }

            state = definition.GetState(stateID);
            return state != null;
        }

        /// <summary>Attempts to retrieve a registered definition without throwing for an unknown identity.</summary>
        public static bool TryGetDefinition(FSMRuntime runtime, int fsmID, out FSM definition)
        {
            if (runtime == null)
            {
                definition = null;
                return false;
            }

            definition = runtime.GetDefinition(fsmID);
            return definition != null;
        }
    }
}
