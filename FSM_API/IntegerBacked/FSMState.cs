using System;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Integer-backed representation of a single FSM state.
    /// </summary>
    /// <remarks>
    /// This is the first atomic building block of the integer-backed FSM surface.
    /// It deliberately contains no string state name. Human-readable names belong
    /// at the translation/authoring boundary; runtime state identity is represented
    /// by <see cref="StateID"/>.
    /// </remarks>
    public class FSMState
    {
        /// <summary>
        /// The integer identity of this state.
        /// </summary>
        public int StateID { get; }

        private Action<IStateContext> _onEnter;
        private Action<IStateContext> _onUpdate;
        private Action<IStateContext> _onExit;

        /// <summary>
        /// Creates an integer-backed state definition.
        /// </summary>
        /// <param name="stateID">The integer identity assigned to the state.</param>
        /// <param name="onEnter">Optional action invoked when entering the state.</param>
        /// <param name="onUpdate">Optional action invoked while updating the state.</param>
        /// <param name="onExit">Optional action invoked when exiting the state.</param>
        public FSMState(
            int stateID,
            Action<IStateContext> onEnter,
            Action<IStateContext> onUpdate,
            Action<IStateContext> onExit)
        {
            StateID = stateID;
            _onEnter = onEnter != null ? onEnter : DefaultOnEnter;
            _onUpdate = onUpdate != null ? onUpdate : DefaultOnUpdate;
            _onExit = onExit != null ? onExit : DefaultOnExit;
        }

        /// <summary>
        /// Replaces the action invoked when entering this state.
        /// </summary>
        public void SetOnEnter(Action<IStateContext> enter)
        {
            _onEnter = enter != null ? enter : DefaultOnEnter;
        }

        /// <summary>
        /// Replaces the action invoked while updating this state.
        /// </summary>
        public void SetOnUpdate(Action<IStateContext> update)
        {
            _onUpdate = update != null ? update : DefaultOnUpdate;
        }

        /// <summary>
        /// Replaces the action invoked when exiting this state.
        /// </summary>
        public void SetOnExit(Action<IStateContext> exit)
        {
            _onExit = exit != null ? exit : DefaultOnExit;
        }

        /// <summary>
        /// Invokes the state's enter action.
        /// </summary>
        public void Enter(IStateContext context)
        {
            _onEnter.Invoke(context);
        }

        /// <summary>
        /// Invokes the state's update action.
        /// </summary>
        public void Update(IStateContext context)
        {
            _onUpdate.Invoke(context);
        }

        /// <summary>
        /// Invokes the state's exit action.
        /// </summary>
        public void Exit(IStateContext context)
        {
            _onExit.Invoke(context);
        }

        private static void DefaultOnEnter(IStateContext context) { }
        private static void DefaultOnUpdate(IStateContext context) { }
        private static void DefaultOnExit(IStateContext context) { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"FSMState: {StateID}";
        }
    }
}
