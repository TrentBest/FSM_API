using System;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// This represents a single **"state"** within your FSM (Finite State Machine) blueprint. 🌟
    /// Think of it as a specific condition or behavior an object can be in, like "Standing," "Jumping," or "Attacking."
    /// </summary>
    /// <remarks>
    /// Each `FSMState` has a unique name and can have three special "actions" linked to it:
    /// <list type="bullet">
    ///     <item><term>OnEnter:</term><description>What happens **when you first arrive** in this state.</description></item>
    ///     <item><term>OnUpdate:</term><description>What happens **repeatedly while you are** in this state.</description></item>
    ///     <item><term>OnExit:</term><description>What happens **just before you leave** this state.</description></item>
    /// </list>
    /// <para>
    /// These actions receive an <see cref="IStateContext"/>, which is like a data bag 🎒 containing
    /// all the specific information your FSM instance needs to perform its actions.
    /// </para>
    /// <para>
    /// While you'll typically set up your states easily using a builder tool, this class is available
    /// for advanced users who might need to create or change states directly in their code.
    /// </para>
    /// </remarks>
    public class FSMState
    {
        /// <summary>
        /// This is the **unique name** for this state.
        /// Once you create a state with a name, it can't be changed.
        /// </summary>
        public string Name { get; }

        private Action<IStateContext> _onEnter;

        /// <summary>
        /// Sets the action that will run **when the FSM first moves into this state.**
        /// </summary>
        /// <remarks>
        /// This action receives a `context` object that provides all the necessary
        /// data for this FSM instance. If you don't provide an action (leave it `null`),
        /// nothing special will happen when the FSM enters this state.
        /// </remarks>
        /// <param name="enter">The action (a piece of code) to run when entering, or `null` if no action is needed.</param>
        public void SetOnEnter(Action<IStateContext> enter) { _onEnter = enter != null ? enter : defaultOnEnter; }

        private Action<IStateContext> _onUpdate;

        /// <summary>
        /// Sets the action that will run **repeatedly while the FSM is currently in this state.**
        /// </summary>
        /// <remarks>
        /// This action receives a `context` object with the FSM's current data.
        /// If you don't provide an action (leave it `null`), nothing special will
        /// happen during each update cycle while in this state.
        /// </remarks>
        /// <param name="update">The action (a piece of code) to run during updates, or `null` if no action is needed.</param>
        public void SetOnUpdate(Action<IStateContext> update) { _onUpdate = update != null ? update : defaultOnUpdate; }

        private Action<IStateContext> _onExit;

        /// <summary>
        /// Sets the action that will run **just before the FSM leaves this state.**
        /// </summary>
        /// <remarks>
        /// This action receives a `context` object with the FSM's current data.
        /// If you don't provide an action (leave it `null`), nothing special will
        /// happen when the FSM exits this state.
        /// </remarks>
        /// <param name="exit">The action (a piece of code) to run when exiting, or `null` if no action is needed.</param>
        public void SetOnExit(Action<IStateContext> exit) { _onExit = exit != null ? exit : defaultOnExit; }

        /// <summary>
        /// Creates a new state definition for your FSM blueprint.
        /// </summary>
        /// <remarks>
        /// When creating a state, you must give it a unique `name`.
        /// You can also optionally provide actions to run `onEnter`, `onUpdate`, and `onExit`.
        /// If you don't provide an action for any of these, it will simply do nothing.
        /// </remarks>
        /// <param name="name">The unique name for this state (e.g., "Idle", "Running").</param>
        /// <param name="onEnter">An optional action to run when the FSM enters this state. Can be `null`.</param>
        /// <param name="onUpdate">An optional action to run repeatedly while in this state. Can be `null`.</param>
        /// <param name="onExit">An optional action to run when the FSM leaves this state. Can be `null`.</param>
        /// <exception cref="ArgumentException">
        /// This happens if you try to create a state without a proper `name` (it's empty or just spaces).
        /// </exception>
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
        /// Runs the 'OnEnter' action for this state.
        /// </summary>
        /// <remarks>
        /// The FSM system calls this method automatically when an FSM instance transitions into this state.
        /// After this method runs, the state's <see cref="IStateContext.HasEnteredCurrentState"/> flag is set to `true`.
        /// </remarks>
        /// <param name="c">The context object specific to this FSM instance, providing data access.</param>
        public void Enter(IStateContext c)
        {
            _onEnter.Invoke(c); // Guaranteed not to be null due to constructor and SetOnX methods
            c.HasEnteredCurrentState = true;
        }

        /// <summary>
        /// Runs the 'OnUpdate' action for this state.
        /// </summary>
        /// <remarks>
        /// The FSM system typically calls this method regularly while an FSM instance
        /// is staying in this state, often as part of a game's update loop.
        /// </remarks>
        /// <param name="c">The context object specific to this FSM instance, providing data access.</param>
        public void Update(IStateContext c)
        {
            _onUpdate.Invoke(c); // Guaranteed not to be null
        }

        /// <summary>
        /// Runs the 'OnExit' action for this state.
        /// </summary>
        /// <remarks>
        /// The FSM system calls this method automatically just before an FSM instance
        /// transitions out of this state to another.
        /// </remarks>
        /// <param name="c">The context object specific to this FSM instance, providing data access.</param>
        public void Exit(IStateContext c)
        {
            _onExit.Invoke(c); // Guaranteed not to be null
        }

        /// <summary>
        /// Gives a simple text description of this state.
        /// </summary>
        /// <returns>A string that looks like "FSMState: [StateName]".</returns>
        public override string ToString()
        {
            return $"FSMState: {Name}";
        }
    }
}