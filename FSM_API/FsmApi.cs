using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Explicit conversion between the independent string-backed and integer-backed FSM runtimes.
    /// </summary>
    public static partial class FsmApi
    {
        public static class Transfer
        {
            /// <summary>Compiles a string-backed definition to integer-backed form with a deterministic state-name mapping.</summary>
            public static IntegerBacked.FSMCompilationResult ToInteger(FSM definition, int fsmID, int processingGroupID = 0)
            {
                return IntegerBacked.FSMCompiler.Compile(definition, fsmID, processingGroupID);
            }

            /// <summary>Reconstructs a string-backed definition using caller-supplied state names.</summary>
            public static FSM ToString(
                IntegerBacked.FSM definition,
                string fsmName,
                string processingGroup,
                IReadOnlyDictionary<int, string> stateIDToName)
            {
                return IntegerBacked.FSMCompiler.Decompile(definition, fsmName, processingGroup, stateIDToName);
            }
        }
    }
}
