using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Public entry point for the native integer-backed FSM surface.
    /// </summary>
    /// <remarks>
    /// This facade intentionally lives beside, rather than inside, the string-backed
    /// <c>FsmApi</c>. Integer callers therefore remain in integer land from definition
    /// through runtime execution. No string registry or string-backed definition is
    /// consulted by these operations.
    /// </remarks>
    public static class FsmApi
    {
        /// <summary>Gets the process-wide integer-backed runtime.</summary>
        public static FSMRuntime Runtime { get; } = new FSMRuntime();

        /// <summary>Integer-backed definition creation operations.</summary>
        public static class Create
        {
            /// <summary>Starts building an integer-backed FSM definition.</summary>
            public static FSMBuilderInt CreateFiniteStateMachine(
                int fsmID,
                int processRate = 0,
                int processingGroupID = 0)
            {
                return new FSMBuilderInt(fsmID, processRate, processingGroupID);
            }

            /// <summary>Registers an already-built integer-backed FSM definition.</summary>
            public static FSM RegisterDefinition(FSM definition)
            {
                Runtime.Register(definition);
                return definition;
            }

            /// <summary>Creates a live integer-backed FSM instance.</summary>
            public static FSMHandleInt CreateInstance(int fsmID, IStateContext context)
            {
                return Runtime.CreateInstance(fsmID, context);
            }
        }

        /// <summary>Integer-backed runtime interaction operations.</summary>
        public static class Interaction
        {
            /// <summary>Updates definitions in one integer processing group.</summary>
            public static void Update(int processingGroupID)
            {
                Runtime.Update(processingGroupID);
            }

            /// <summary>Updates all eligible integer-backed definitions.</summary>
            public static void UpdateAll()
            {
                Runtime.UpdateAll();
            }

            /// <summary>Removes a live integer-backed instance.</summary>
            public static bool DestroyInstance(FSMHandleInt handle)
            {
                return Runtime.RemoveInstance(handle);
            }

            /// <summary>Unregisters an integer-backed definition and its live instances.</summary>
            public static bool Unregister(int fsmID)
            {
                return Runtime.Unregister(fsmID);
            }

            /// <summary>Returns whether an integer-backed definition is registered.</summary>
            public static bool Contains(int fsmID)
            {
                return Runtime.Contains(fsmID);
            }

            /// <summary>Gets a registered integer-backed definition.</summary>
            public static FSM GetDefinition(int fsmID)
            {
                return Runtime.GetDefinition(fsmID);
            }
        }
    }
}
