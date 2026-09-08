using System;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Explicit integer-backed runtime handle for an FSM instance.
    /// </summary>
    /// <remarks>
    /// The integer-backed API intentionally exposes a distinct handle type so callers cannot accidentally
    /// cross the representation boundary. <see cref="CurrentStateID"/> is the authoritative current-state
    /// value; no string state name is exposed by this handle.
    ///
    /// This type inherits the integer-only execution behavior from <see cref="FSMHandle"/>. The inherited
    /// base remains available as a compatibility surface for the initial IntegerBacked implementation,
    /// while new runtime instances are created as <see cref="FSMHandleInt"/>.
    /// </remarks>
    public sealed class FSMHandleInt : FSMHandle
    {
        /// <summary>Creates an explicitly typed integer-backed FSM handle.</summary>
        /// <param name="definition">Integer-backed FSM definition.</param>
        /// <param name="context">Context for this FSM instance.</param>
        /// <param name="id">Unique runtime handle identity.</param>
        public FSMHandleInt(FSM definition, IStateContext context, int id = -1)
            : base(definition, context, id)
        {
        }
    }
}
