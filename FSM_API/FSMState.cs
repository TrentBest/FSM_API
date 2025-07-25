using System;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Represents a single state within a Finite State Machine (FSM) definition.
    /// <para>
    /// An FSM State defines a unique name and specifies actions to be executed at key lifecycle points:
    /// when entering the state (<c>OnEnter</c>), while remaining in the state (<c>OnUpdate</c>),
    /// and when exiting the state (<c>OnExit</c>). These actions operate on the <see cref="IStateContext"/>
    /// instance associated with the FSM.
    /// </para>
    /// <para>
    /// While states are typically defined and configured via fluent builders, this class is publicly exposed
    /// to enable advanced users (e.g., for custom FSM definition loading, runtime modification of state
    /// behaviors, or specific performance optimizations) to directly create and manipulate state objects.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Each <c>FSMState</c> object is a part of an FSM's definition. The actions associated with a state
    /// are invoked by the FSM system during state transitions and processing cycles.
    /// </remarks>
    public class FSMState
    {
        /// <summary>
        /// Gets the unique name of this state. The name is immutable once the state is created.
        /// </summary>
        public string Name { get; }

        private Action<IStateContext> _onEnter;
        /// <summary>
        /// Sets the action to be executed when the FSM transitions into this state.
        /// This action receives the <see cref="IStateContext"/> instance for state-specific data access and manipulation.
        /// If no action is provided (i.e., <c>null</c>), a default no-operation action is used.
        /// </summary>
        /// <param name="enter">The action to execute on state entry, or <c>null</c> for no action.</param>
        public void SetOnEnter(Action<IStateContext> enter) { _onEnter = enter != null ? enter : defaultOnEnter; }

        private Action<IStateContext> _onUpdate;
        /// <summary>
        /// Sets the action to be executed periodically while the FSM is in this state.
        /// This action receives the <see cref="IStateContext"/> instance for state-specific data access and manipulation.
        /// If no action is provided (i.e., <c>null</c>), a default no-operation action is used.
        /// </summary>
        /// <param name="update">The action to execute during state updates, or <c>null</c> for no action.</param>
        public void SetOnUpdate(Action<IStateContext> update) { _onUpdate = update != null ? update : defaultOnUpdate; }

        private Action<IStateContext> _onExit;
        /// <summary>
        /// Sets the action to be executed when the FSM transitions out of this state.
        /// This action receives the <see cref="IStateContext"/> instance for state-specific data access and manipulation.
        /// If no action is provided (i.e., <c>null</c>), a default no-operation action is used.
        /// </summary>
        /// <param name="exit">The action to execute on state exit, or <c>null</c> for no action.</param>
        public void SetOnExit(Action<IStateContext> exit) { _onExit = exit != null ? exit : defaultOnExit; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FSMState"/> class.
        /// </summary>
        /// <param name="name">The unique name for this state.</param>
        /// <param name="onEnter">An optional action to execute when entering this state. Can be null.</param>
        /// <param name="onUpdate">An optional action to execute periodically while in this state. Can be null.</param>
        /// <param name="onExit">An optional action to execute when exiting this state. Can be null.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="name"/> is null, empty, or whitespace.</exception>
        public FSMState(string name, Action<IStateContext> onEnter, Action<IStateContext> onUpdate, Action<IStateContext> onExit)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("State name cannot be null or empty.", nameof(name));
            }

            Name = name;

            // Assign default no-op actions if null to avoid null checks during invocation.
            _onEnter = onEnter != null ? onEnter : defaultOnEnter;
            _onUpdate = onUpdate != null ? onUpdate : defaultOnUpdate;
            _onExit = onExit != null ? onExit : defaultOnExit;
        }

        private void defaultOnExit(IStateContext context) { }
        private void defaultOnUpdate(IStateContext context) { }
        private void defaultOnEnter(IStateContext context) { }

        /// <summary>
        /// Executes the 'onEnter' action for this state.
        /// This method is typically called by the FSM when transitioning into this state.
        /// </summary>
        /// <param name="c">The state context providing access to FSM instance data.</param>
        public void Enter(IStateContext c)
        {
            _onEnter.Invoke(c); // Guaranteed not to be null due to constructor and SetOnX methods
        }

        /// <summary>
        /// Executes the 'onUpdate' action for this state.
        /// This method is typically called periodically by the FSM while it is in this state.
        /// </summary>
        /// <param name="c">The state context providing access to FSM instance data.</param>
        public void Update(IStateContext c)
        {
            _onUpdate.Invoke(c); // Guaranteed not to be null
        }

        /// <summary>
        /// Executes the 'onExit' action for this state.
        /// This method is typically called by the FSM when transitioning out of this state.
        /// </summary>
        /// <param name="c">The state context providing access to FSM instance data.</param>
        public void Exit(IStateContext c)
        {
            _onExit.Invoke(c); // Guaranteed not to be null
        }

        /// <summary>
        /// Returns a string representation of the FSMState.
        /// </summary>
        /// <returns>A string in the format "FSMState: [StateName]".</returns>
        public override string ToString()
        {
            return $"FSMState: {Name}";
        }
    }
}