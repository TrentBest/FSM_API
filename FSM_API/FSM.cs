using System;
using System.Collections.Generic;
using System.Linq;


namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Represents a complete Finite State Machine definition, serving as a blueprint
    /// for FSM instances. This class encapsulates the states, transitions between them,
    /// and core configuration properties for an FSM.
    /// </summary>
    /// <remarks>
    /// An <see cref="FSM"/> object defines the behavior of a state machine but does not
    /// maintain its runtime state. Instead, instances of this definition are created and managed
    /// through <see cref="FSMHandle"/> objects via the <see cref="FSM_API"/> system.
    /// This class is part of the internal API mechanics and is typically constructed
    /// and managed by the <see cref="FSMBuilder"/>.
    /// </remarks>
    public class FSM
    {
        /// <summary>
        /// Gets or sets the unique name assigned to this FSM definition.
        /// </summary>
        /// <remarks>
        /// This name is used to register and retrieve the FSM definition within the
        /// <see cref="FSM_API"/> system. It is typically set during the FSM's construction
        /// via the <see cref="FSMBuilder"/>.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the initial state for this FSM definition.
        /// </summary>
        /// <remarks>
        /// When a new FSM instance (<see cref="FSMHandle"/>) is created from this definition,
        /// it will begin its lifecycle by entering the state specified by <see cref="InitialState"/>.
        /// A valid initial state must be added to the FSM definition using <see cref="AddState(FSMState)"/>.
        /// </remarks>
        public string InitialState { get; set; }

        /// <summary>
        /// Gets or sets the desired processing rate for instances created from this FSM definition.
        /// </summary>
        /// <remarks>
        /// This property determines how frequently the FSM's <see cref="Step"/> method
        /// is automatically invoked by the <see cref="FSM_API.Internal.TickAll(string)"/>
        /// method within its designated <see cref="ProcessingGroup"/>.
        /// </remarks>
        /// <value>
        /// <list type="bullet">
        ///    <item><term><c>-1</c></term><description>
        ///        Instances will be processed every time the <see cref="FSM_API.Internal.TickAll(string)"/>
        ///        method is called for this FSM's <see cref="ProcessingGroup"/> (e.g., every frame in a game loop).
        ///    </description></item>
        ///    <item><term><c>0</c></term><description>
        ///        Instances will *not* be automatically ticked by the API's update methods.
        ///        They must be driven externally via explicit event triggers or manual calls
        ///        to <see cref="FSMHandle.Update()"/>.
        ///    </description></item>
        ///    <item><term><c>&gt;0</c></term><description>
        ///        Instances will be processed every Nth call to <see cref="FSM_API.Internal.TickAll(string)"/>,
        ///        where N is the value of <c>ProcessRate</c>. For example, a value of 5 means
        ///        the FSM will update every 5th tick.
        ///    </description></item>
        /// </list>
        /// </value>
        public int ProcessRate { get; internal set; }

        /// <summary>
        /// Gets or sets the name of the processing group this FSM definition belongs to.
        /// </summary>
        /// <remarks>
        /// This property organizes FSM definitions into logical groups, allowing for
        /// selective and controlled updates. The <see cref="FSM_API.Interaction.Update(string)"/>
        /// method is used to manually trigger updates for all FSM instances associated with
        /// a specific <see cref="ProcessingGroup"/> name.
        /// <para>
        /// This design enables hierarchical or nested processing, where a complex system
        /// (e.g., an "Arcade" FSM) might have its own internal update loop that, in turn,
        /// calls <see cref="FSM_API.Interaction.Update(string)"/> for its sub-components
        /// (e.g., a "PacMan" FSM group), providing fine-grained control over execution flow
        /// beyond traditional engine-level update cycles.
        /// </para>
        /// </remarks>
        public string ProcessingGroup { get; internal set; }

        /// <summary>
        /// Internal string identifier used to denote "Any State" transitions.
        /// </summary>
        /// <remarks>
        /// Transitions from this pseudo-state (<c>"__ANY_STATE__"</c>) can be triggered
        /// from any active state in the FSM, effectively acting as global transitions.
        /// This constant provides a unique and unlikely string to prevent collisions with
        /// actual state names.
        /// </remarks>
        public const string AnyStateIdentifier = "__ANY_STATE__"; // Using a unique, unlikely string

        private readonly Dictionary<string, FSMState> _states = new();
        private readonly List<FSMTransition> _transitions = new(); // Regular transitions
        private readonly List<FSMTransition> _anyStateTransitions = new(); // Any State transitions

        /// <summary>
        /// Initializes a new instance of the <see cref="FSM"/> class with default values.
        /// </summary>
        /// <remarks>
        /// While this constructor is public, an <see cref="FSM"/> object is typically
        /// instantiated and configured through the fluent interface provided by
        /// <see cref="FSMBuilder"/> to ensure a complete and valid definition.
        /// Default values: <see cref="Name"/> = "UnnamedFSM", <see cref="InitialState"/> = "__ANY_STATE__",
        /// <see cref="ProcessingGroup"/> = "Update".
        /// </remarks>
        public FSM()
        {
            Name = "UnnamedFSM";
            InitialState = AnyStateIdentifier;
            ProcessingGroup = "Update";
        }

        /// <summary>
        /// Adds or updates a state within this FSM definition.
        /// </summary>
        /// <remarks>
        /// If a state with the same name as the provided <paramref name="s"/> already exists,
        /// its definition will be overwritten. This method allows for dynamic redefinition
        /// of states within an FSM blueprint.
        /// </remarks>
        /// <param name="s">The <see cref="FSMState"/> object to add or update.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="s"/> (state object) is <c>null</c>.
        /// An internal error is also invoked via <see cref="FSM_API.Error.InvokeInternalApiError(string, Exception)"/>.
        /// </exception>
        public void AddState(FSMState s)
        {
            if (s == null)
            {
                FSM_API.Error.InvokeInternalApiError($"Attempted to add a null state to FSM '{Name}'.", new ArgumentNullException(nameof(s)));
                return;
            }

            _states[s.Name] = s;
        }

        /// <summary>
        /// Adds a regular transition between two specific states in the FSM definition.
        /// </summary>
        /// <remarks>
        /// A regular transition is evaluated only when the FSM instance is currently
        /// in the <paramref name="from"/> state. If a transition with the same
        /// <paramref name="from"/> and <paramref name="to"/> states already exists,
        /// it will be replaced with the new definition.
        /// </remarks>
        /// <param name="from">The name of the source state from which the transition can occur.</param>
        /// <param name="to">The name of the target state to which the FSM will transition.</param>
        /// <param name="cond">
        /// The condition function (<see cref="Func{T, TResult}"/>) that must return <c>true</c>
        /// for the transition to be taken. This function receives the current <see cref="IStateContext"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="cond"/> (condition function) is <c>null</c>.
        /// An internal error is also invoked via <see cref="FSM_API.Error.InvokeInternalApiError(string, Exception)"/>.
        /// </exception>
        public void AddTransition(string from, string to, Func<IStateContext, bool> cond)
        {
            if (cond == null)
            {
                FSM_API.Error.InvokeInternalApiError($"Attempted to add a transition with null condition from '{from}' to '{to}' in FSM '{Name}'.", new ArgumentNullException(nameof(cond)));
                return;
            }

            // Remove existing transition if it matches (from, to) pair for clean updates
            _transitions.RemoveAll(t => t.From == from && t.To == to);
            _transitions.Add(new FSMTransition(from, to, cond));
        }

        /// <summary>
        /// Adds a global "Any State" transition to the FSM definition.
        /// </summary>
        /// <remarks>
        /// An "Any State" transition is evaluated regardless of the FSM's current state.
        /// If its condition is met, the FSM will transition to the specified target state.
        /// These transitions are typically checked before regular state-specific transitions.
        /// If an "Any State" transition to the same <paramref name="to"/> state already exists,
        /// it will be replaced with the new definition.
        /// </remarks>
        /// <param name="to">The name of the target state for the "Any State" transition.</param>
        /// <param name="cond">
        /// The condition function (<see cref="Func{T, TResult}"/>) that must return <c>true</c>
        /// for the transition to be taken. This function receives the current <see cref="IStateContext"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="cond"/> (condition function) is <c>null</c>.
        /// An internal error is also invoked via <see cref="FSM_API.Error.InvokeInternalApiError(string, Exception)"/>.
        /// </exception>
        public void AddAnyStateTransition(string to, Func<IStateContext, bool> cond)
        {
            if (cond == null)
            {
                FSM_API.Error.InvokeInternalApiError($"Attempted to add an Any-State transition with null condition to '{to}' in FSM '{Name}'.", new ArgumentNullException(nameof(cond)));
                return;
            }

            // Remove existing any-state transition if it matches 'to' state for clean updates
            _anyStateTransitions.RemoveAll(t => t.To == to);
            _anyStateTransitions.Add(new FSMTransition(AnyStateIdentifier, to, cond));
        }

        /// <summary>
        /// Checks if a state with the specified name exists within this FSM definition.
        /// </summary>
        /// <param name="stateName">The name of the state to query for existence.</param>
        /// <returns><c>true</c> if a state with <paramref name="stateName"/> is defined in this FSM; otherwise, <c>false</c>.</returns>
        public bool HasState(string stateName)
        {
            return _states.ContainsKey(stateName);
        }

        /// <summary>
        /// Retrieves a read-only collection of all <see cref="FSMState"/> objects defined in this FSM.
        /// </summary>
        /// <returns>
        /// A <see cref="IReadOnlyCollection{T}"/> of <see cref="FSMState"/> objects.
        /// The returned collection is a snapshot and cannot be modified directly.
        /// </returns>
        public IReadOnlyCollection<FSMState> GetAllStates()
        {
            return _states.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// Retrieves a read-only collection of all transitions (both regular and "Any State" transitions)
        /// defined within this FSM.
        /// </summary>
        /// <returns>
        /// A <see cref="IReadOnlyCollection{T}"/> of <see cref="FSMTransition"/> objects.
        /// The returned collection is a snapshot and cannot be modified directly.
        /// </returns>
        public IReadOnlyCollection<FSMTransition> GetAllTransitions()
        {
            // Combine regular and any-state transitions
            var allTransitions = new List<FSMTransition>(_transitions);
            allTransitions.AddRange(_anyStateTransitions);
            return allTransitions.AsReadOnly();
        }

        /// <summary>
        /// Enters the <see cref="InitialState"/> of the FSM for a given context.
        /// </summary>
        /// <remarks>
        /// This method is typically invoked internally when an <see cref="FSMHandle"/>
        /// is first created and needs to establish its starting state. It calls the
        /// <see cref="FSMState.Enter(IStateContext)"/> method of the initial state.
        /// </remarks>
        /// <param name="ctx">The context object (<see cref="IStateContext"/>) specific to the FSM instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <see cref="InitialState"/> specified in the FSM definition
        /// does not correspond to an existing state. This indicates a configuration error
        /// within the FSM blueprint. An internal error is also invoked.
        /// </exception>
        public void EnterInitial(IStateContext ctx)
        {
            if (!_states.TryGetValue(InitialState, out var state))
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"Initial state '{InitialState}' not found for FSM '{Name}'. This indicates a corrupted FSM definition.",
                    new ArgumentException($"Initial state '{InitialState}' not found for FSM '{Name}'.", nameof(InitialState))
                );
                // Even though we invoke an error, we still throw to prevent the FSM from operating in an invalid state.
                throw new ArgumentException($"Initial state '{InitialState}' not found for FSM '{Name}'.", nameof(InitialState));
            }
            state.Enter(ctx);
        }

        /// <summary>
        /// Executes a single step (tick) of the FSM for a given context, determining
        /// and potentially transitioning to the next state.
        /// </summary>
        /// <remarks>
        /// The step logic follows a specific order:
        /// <list type="number">
        ///     <item><description>
        ///         **Any-State Transitions:** All <see cref="AddAnyStateTransition(string, Func{IStateContext, bool})"/>
        ///         conditions are evaluated first. If an "Any State" transition's condition is met,
        ///         the FSM immediately transitions to the target state, and the method returns.
        ///         Errors during condition evaluation or to non-existent target states are logged
        ///         internally, but the process continues or the transition is skipped.
        ///     </description></item>
        ///     <item><description>
        ///         **Current State Update:** The <see cref="FSMState.Update(IStateContext)"/> method
        ///         of the <paramref name="current"/> state is invoked. Exceptions during this execution
        ///         are caught and reported via <see cref="FSM_API.Error.InvokeInternalApiError(string, Exception)"/>.
        ///     </description></item>
        ///     <item><description>
        ///         **Regular Transitions:** All transitions defined from the <paramref name="current"/> state
        ///         via <see cref="AddTransition(string, string, Func{IStateContext, bool})"/> are evaluated.
        ///         The first transition whose condition returns <c>true</c> will cause the FSM to transition
        ///         to its target state, and the method returns. Errors are handled similarly to Any-State transitions.
        ///     </description></item>
        /// </list>
        /// If the <paramref name="current"/> state is not found in the FSM definition, an internal error is logged,
        /// and the FSM attempts to force a transition back to its <see cref="InitialState"/> as a recovery measure.
        /// </remarks>
        /// <param name="current">The name of the FSM instance's current state.</param>
        /// <param name="ctx">The context object (<see cref="IStateContext"/>) for the FSM instance.</param>
        /// <param name="next">
        /// An output parameter that will contain the name of the state the FSM is in after this step.
        /// This will be the same as <paramref name="current"/> if no transition occurs.
        /// </param>
        public void Step(string current, IStateContext ctx, out string next)
        {
            next = current; // Assume state doesn't change unless a transition fires

            if (!_states.TryGetValue(current, out var currentState))
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"FSM '{Name}' instance in processing group '{ProcessingGroup}' has an invalid current state '{current}'. Attempting to recover by transitioning to initial state '{InitialState}'.", null
                );
                // Attempt to recover by transitioning to the initial state
                ForceTransition(current, InitialState, ctx);
                next = InitialState;
                return;
            }

            // 1. Check Any-State Transitions First (higher priority for global interrupts)
            foreach (var t in _anyStateTransitions)
            {
                // Check if the target state exists before evaluating condition
                if (!_states.ContainsKey(t.To))
                {
                    FSM_API.Error.InvokeInternalApiError(
                        $"FSM '{Name}' Any-State transition defined to non-existent state '{t.To}'. Transition skipped for safety.",
                        null
                    );
                    continue; // Skip this problematic transition
                }

                try
                {
                    if (t.Condition(ctx))
                    {
                        currentState.Exit(ctx);
                        _states[t.To].Enter(ctx);
                        next = t.To;
                        return; // Transition occurred, exit
                    }
                }
                catch (Exception ex)
                {
                    FSM_API.Error.InvokeInternalApiError(
                        $"Error evaluating Any-State transition condition from '{current}' to '{t.To}' in FSM '{Name}'. Exception: {ex.Message}",
                        ex
                    );
                    // Continue to next transition or state update
                }
            }

            // 2. Execute current state's Update logic
            try
            {
                currentState.Update(ctx);
            }
            catch (Exception ex)
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"Error during Update logic of state '{current}' in FSM '{Name}'. Exception: {ex.Message}",
                    ex
                );
                // Continue to check transitions, or let the FSMHandle catch this as a fubar
            }


            // 3. Check regular transitions from the current state
            foreach (var t in _transitions)
            {
                if (t.From == current) // Only consider transitions *from* the current state
                {
                    // Check if the target state exists before evaluating condition
                    if (!_states.ContainsKey(t.To))
                    {
                        FSM_API.Error.InvokeInternalApiError(
                            $"FSM '{Name}' regular transition defined from '{current}' to non-existent state '{t.To}'. Transition skipped for safety.",
                            null
                        );
                        continue; // Skip this problematic transition
                    }

                    try
                    {
                        if (t.Condition(ctx))
                        {
                            currentState.Exit(ctx);
                            _states[t.To].Enter(ctx);
                            next = t.To;
                            return; // Transition occurred, exit
                        }
                    }
                    catch (Exception ex)
                    {
                        FSM_API.Error.InvokeInternalApiError(
                            $"Error evaluating regular transition condition from '{current}' to '{t.To}' in FSM '{Name}'. Exception: {ex.Message}",
                            ex
                        );
                        // Continue to next transition
                    }
                }
            }
        }

        /// <summary>
        /// Forces an immediate transition of an FSM instance from a specified state to a new target state,
        /// bypassing any transition conditions.
        /// </summary>
        /// <remarks>
        /// This method executes the <see cref="FSMState.Exit(IStateContext)"/> action of the
        /// <paramref name="from"/> state (if that state exists) and then the
        /// <see cref="FSMState.Enter(IStateContext)"/> action of the <paramref name="to"/> state.
        /// It's useful for external control or recovery mechanisms.
        /// Errors during state exit or entry actions are caught and reported via
        /// <see cref="FSM_API.Error.InvokeInternalApiError(string, Exception)"/>.
        /// </remarks>
        /// <param name="from">
        /// The name of the state to exit from. If this state does not exist or is <c>null</c>/empty,
        /// no exit action will be performed, but the method will still attempt to enter the <paramref name="to"/> state.
        /// </param>
        /// <param name="to">The name of the state to enter into. This state must exist in the FSM definition.</param>
        /// <param name="ctx">The context object (<see cref="IStateContext"/>) for the FSM instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the target <paramref name="to"/> state does not exist in the FSM definition.
        /// An internal error is also invoked in this case.
        /// </exception>
        public void ForceTransition(string from, string to, IStateContext ctx)
        {
            // Check if the "from" state actually exists and handle gracefully if not
            // (e.g., initial state wasn't entered properly or FSM was in an invalid state).
            // We still try to call Exit if possible.
            if (!string.IsNullOrEmpty(from) && _states.TryGetValue(from, out var fromState))
            {
                try
                {
                    fromState.Exit(ctx);
                }
                catch (Exception ex)
                {
                    FSM_API.Error.InvokeInternalApiError(
                        $"Error during Exit logic of state '{from}' during forced transition to '{to}' in FSM '{Name}'. Exception: {ex.Message}",
                        ex
                    );
                    // Do not re-throw, continue to enter the new state if possible.
                }
            }
            else if (!string.IsNullOrEmpty(from)) // Only log if 'from' was specified but not found
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"Attempted to force transition from non-existent state '{from}' for FSM '{Name}'. Exiting original state skipped.",
                    null
                );
            }

            if (!_states.TryGetValue(to, out var toState))
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"Target state '{to}' for forced transition does not exist in FSM '{Name}'. Forced transition failed.",
                    new ArgumentException($"Target state '{to}' for forced transition does not exist in FSM '{Name}'.", nameof(to))
                );
                throw new ArgumentException($"Target state '{to}' for forced transition does not exist in FSM '{Name}'.", nameof(to));
            }

            try
            {
                toState.Enter(ctx);
            }
            catch (Exception ex)
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"Error during Enter logic of state '{to}' during forced transition from '{from}' in FSM '{Name}'. Exception: {ex.Message}",
                    ex
                );
                throw; // Re-throw, as the FSM might now be in a partially entered state.
            }
        }

        public void RemoveState(string name)
        {
            _states.Remove(name);
        }

        public bool HasTransition(string fromState, string toState)
        {
            return _transitions.Any(s=>s.From == fromState && s.To == toState);
        }

        public void RemoveTransition(string from, string to)
        {
            if(HasTransition(from, to))
            {
                var fsmTransition = _transitions.First(s=>s.From == from && s.To == to);
                _transitions.Remove(fsmTransition);
            }
        }
    }
}