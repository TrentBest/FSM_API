using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Think of an **FSM** (Finite State Machine) as a master blueprint 🗺️.
    /// It lays out all the possible behaviors and rules for how something
    /// should act over time. This blueprint defines all the different
    /// "states" it can be in, like "Idle," "Walking," or "Attacking,"
    /// and how it moves between these states.
    /// Think of an **FSM** (Finite State Machine) as a master blueprint 🗺️.
    /// It lays out all the possible behaviors and rules for how something
    /// should act over time. This blueprint defines all the different
    /// "states" it can be in, like "Idle," "Walking," or "Attacking,"
    /// and how it moves between these states.
    /// <div class="mermaid">
    /// graph TD
    ///     A[FSM Definition] --> B(States);
    ///     A --> C(Transitions);
    ///     A --> D(Initial State);
    /// </div>
    /// </summary>
    /// <remarks>
    /// This blueprint itself doesn't actually *do* anything directly.
    /// Instead, we use it to create **instances** or "copies" of the FSM.
    /// Each copy is called an <see cref="FSMHandle"/> and is managed by
    /// our <see cref="FSM_API"/> system.
    /// <para>
    /// This `FSM` class is mainly for the internal workings of the API.
    /// You usually won't create these directly. Instead, you'll use
    /// the <see cref="FSMBuilder"/> to easily design and build your FSM blueprints.
    /// </para>
    /// </remarks>
    public class FSM
    {
        /// <summary>
        /// This is the **unique name** for this specific FSM blueprint.
        /// </summary>
        /// <remarks>
        /// This name helps the <see cref="FSM_API"/> system find and use
        /// this FSM definition. You typically set this name when you're
        /// building your FSM using the <see cref="FSMBuilder"/>.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// This is the name of the **first state** your FSM will enter
        /// when it starts up.
        /// </summary>
        /// <remarks>
        /// When you create a new FSM instance (an <see cref="FSMHandle"/>)
        /// from this blueprint, it will always begin in the state specified here.
        /// Make sure this state has actually been added to your FSM definition
        /// using the <see cref="AddState(FSMState)"/> method.
        /// </remarks>
        public string InitialState { get; set; }

        /// <summary>
        /// This controls how often instances of this FSM blueprint will automatically
        /// "think" or "update" themselves.
        /// </summary>
        /// <remarks>
        /// This number tells the <see cref="FSM_API.Internal.TickAll(string)"/>
        /// method how frequently to call the FSM's <see cref="Step"/> method
        /// within its assigned <see cref="ProcessingGroup"/>.
        /// </remarks>
        /// <value>
        /// <list type="bullet">
        ///      <item><term><c>-1</c></term><description>
        ///          The FSM will update **every single time** its
        ///          <see cref="ProcessingGroup"/> is "ticked"
        ///          (e.g., every frame in a game). This is the most frequent update.
        ///      </description></item>
        ///      <item><term><c>0</c></term><description>
        ///          The FSM will **NOT** update automatically. You'll need
        ///          to manually tell it to update using events or by directly
        ///          calling <see cref="FSMHandle.Update(string)"/>.
        ///      </description></item>
        ///      <item><term><c>&gt;0</c></term><description>
        ///          The FSM will update **every Nth tick**, where N is this number.
        ///          For example, if it's 5, it updates every 5th time
        ///          <see cref="FSM_API.Internal.TickAll(string)"/> is called.
        ///      </description></item>
        /// </list>
        /// </value>
        public int ProcessRate { get; internal set; }

        /// <summary>
        /// This is the **category name** that this FSM blueprint belongs to.
        /// </summary>
        /// <remarks>
        /// Processing groups help you organize your FSMs and control when
        /// they update. You can manually trigger all FSM instances within a
        /// specific <see cref="ProcessingGroup"/> to update by calling
        /// <see cref="FSM_API.Interaction.Update(string)"/>.
        /// <para>
        /// Imagine you have a large system, like an "Arcade" FSM. You could
        /// give it its own update cycle, and within that cycle, it might
        /// tell all "PacMan" FSMs (which are in their own processing group)
        /// to update. This gives you very precise control over how your
        /// FSMs run.
        /// </para>
        /// </remarks>
        public string ProcessingGroup { get; internal set; }



        /// <summary>
        /// Creates a new, empty FSM blueprint with some default settings.
        /// </summary>
        /// <remarks>
        /// While you *can* use this directly, it's generally recommended
        /// to use the <see cref="FSMBuilder"/> to create and set up your FSMs.
        /// The builder provides an easier, step-by-step way to ensure your
        /// FSM is complete and valid.
        /// <para>
        /// Default values when created:
        /// <list type="bullet">
        ///      <item><term><see cref="Name"/></term><description> "UnnamedFSM"</description></item>
        ///      <item><term><see cref="InitialState"/></term><description> "__ANY_STATE__" (a placeholder, should be set to a real state)</description></item>
        ///      <item><term><see cref="ProcessingGroup"/></term><description> "Update"</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public FSM()
        {
            Name = "UnnamedFSM";
            InitialState = AnyStateIdentifier;
            ProcessingGroup = "Update";
        }

        /// <summary>
        /// Adds a **state** to this FSM blueprint, or updates it if a state
        /// with the same name already exists.
        /// </summary>
        /// <remarks>
        /// If you add a state whose name is already in the FSM, the old
        /// definition will be completely replaced by the new one you provide.
        /// This lets you change state behaviors even after the FSM is defined.
        /// </remarks>
        /// <param name="s">The <see cref="FSMState"/> object you want to add or update.</param>
        /// <exception cref="ArgumentNullException">
        /// This happens if you try to add a state that doesn't exist (it's `null`).
        /// An internal API error will also be triggered to help with debugging.
        /// </exception>
        public void AddState(FSMState s)
        {
            if (s == null)
            {
                FSM_API.Error.InvokeInternalApiError($"Attempted to add a null state to FSM '{Name}'.", new ArgumentNullException(nameof(s)));
                return;
            }

            _states[s.Name] = s;
            if (_states.Count == 1)
            {
                // If this is the first state added, set it as the initial state
                InitialState = s.Name;
            }
        }

        /// <summary>
        /// Adds a **normal transition** between two specific states in your FSM blueprint.
        /// </summary>
        /// <remarks>
        /// A "normal" transition only gets checked if your FSM is currently
        /// in the <paramref name="from"/> state. If you add a transition
        /// that already exists between the same <paramref name="from"/> and
        /// <paramref name="to"/> states, the new one you define will
        /// replace the old one.
        /// </remarks>
        /// <param name="from">The name of the starting state for this transition.</param>
        /// <param name="to">The name of the state the FSM will move to if this transition happens.</param>
        /// <param name="cond">
        /// This is a **condition** (a function that returns `true` or `false`).
        /// The FSM will only move from <paramref name="from"/> to <paramref name="to"/>
        /// if this condition is `true`. This function receives information about the
        /// current FSM situation through an <see cref="IStateContext"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// This happens if you try to add a transition without a condition (the `cond` function is `null`).
        /// An internal API error will also be triggered.
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
        /// Adds a **global "Any State" transition** to your FSM blueprint.
        /// </summary>
        /// <remarks>
        /// An "Any State" transition is special because it's checked **no matter
        /// what state** your FSM is currently in. If its condition is met,
        /// the FSM will immediately switch to the <paramref name="to"/> state.
        /// These "Any State" transitions are usually checked *before* any
        /// regular, state-specific transitions. If you define an "Any State"
        /// transition to a state that already has one, the new definition
        /// will replace the old one.
        /// </remarks>
        /// <param name="to">The name of the state the FSM will move to if this "Any State" transition happens.</param>
        /// <param name="cond">
        /// This is a **condition** (a function that returns `true` or `false`).
        /// The FSM will only move to <paramref name="to"/> if this condition is `true`.
        /// This function receives information about the current FSM situation
        /// through an <see cref="IStateContext"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// This happens if you try to add an "Any State" transition without a condition (the `cond` function is `null`).
        /// An internal API error will also be triggered.
        /// </exception>
        public void AddAnyStateTransition(string to, Func<IStateContext, bool> cond)
        {
            if (cond == null)
            {
                FSM_API.Error.InvokeInternalApiError($"Attempted to add an Any-State transition with null condition to '{to}' in FSM '{Name}'.", new ArgumentNullException(nameof(cond)));
                return;
            }
            if (to == string.Empty)
            {
                FSM_API.Error.InvokeInternalApiError($"Attempted to add an Any-State transition with empty string 'to' in FSM '{Name}'.", new ArgumentNullException(nameof(cond)));
                return;
            }
            // Remove existing any-state transition if it matches 'to' state for clean updates
            _anyStateTransitions.RemoveAll(t => t.To == to);
            _anyStateTransitions.Add(new FSMTransition(AnyStateIdentifier, to, cond));
        }


        /// <summary>
        /// Adds a global "Any State" transition to your FSM blueprint using an existing <see cref="FSMTransition"/> object.
        /// </summary>
        /// <remarks>
        /// This method allows for adding or updating "Any State" transitions, which are evaluated
        /// regardless of the FSM's current state. If a transition to the same <see cref="FSMTransition.To"/>
        /// state already exists as an "Any State" transition, it will be replaced by the provided <paramref name="transition"/>.
        /// </remarks>
        /// <param name="transition">The <see cref="FSMTransition"/> object representing the "Any State" transition to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="transition"/> is `null`.
        /// An internal API error will also be triggered.
        /// </exception>
        public void AddAnyStateTransition(FSMTransition transition)
        {
            if (transition == null)
            {
                FSM_API.Error.InvokeInternalApiError($"Attempted to add a null Any-State transition to  in FSM '{Name}'.", new ArgumentNullException(nameof(transition)));
                return;
            }
            _anyStateTransitions.RemoveAll(t => t.To == transition.To);
            _anyStateTransitions.Add(transition);
        }

        /// <summary>
        /// Retrieves a list of all "Any State" transitions defined for this FSM blueprint.
        /// </summary>
        /// <remarks>
        /// These transitions are special as they are evaluated irrespective of the FSM's
        /// current state.
        /// </remarks>
        /// <returns>A <see cref="List{FSMTransition}"/> containing all "Any State" transitions. Returns an empty list if none are defined.</returns>

        public List<FSMTransition> GetAnyStateTransitions()
        {
            return _anyStateTransitions;
        }


        /// <summary>
        /// Checks if a state with the given name exists in this FSM blueprint.
        /// </summary>
        /// <param name="stateName">The name of the state you want to check for.</param>
        /// <returns>
        /// <c>true</c> if a state with <paramref name="stateName"/> is part of this FSM's definition;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool HasState(string stateName)
        {
            return _states.ContainsKey(stateName);
        }

        /// <summary>
        /// Gets a list of all the **states** defined in this FSM blueprint.
        /// </summary>
        /// <returns>
        /// A **read-only list** of all <see cref="FSMState"/> objects.
        /// You can look at them, but you can't change the FSM's states directly
        /// using this list.
        /// </returns>
        public IReadOnlyCollection<FSMState> GetAllStates()
        {
            return _states.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets a list of **all transitions** (both normal and "Any State" transitions)
        /// defined within this FSM blueprint.
        /// </summary>
        /// <returns>
        /// A **read-only list** of all <see cref="FSMTransition"/> objects.
        /// You can examine these transitions, but you can't modify them
        /// directly through this list.
        /// </returns>
        public IReadOnlyCollection<FSMTransition> GetAllTransitions()
        {
            // Combine regular and any-state transitions
            var allTransitions = new List<FSMTransition>(_transitions);
            allTransitions.AddRange(_anyStateTransitions);
            return allTransitions.AsReadOnly();
        }

        /// <summary>
        /// Tells the FSM instance to **enter its initial state**.
        /// </summary>
        /// <remarks>
        /// This method is usually called internally by the system right
        /// after an <see cref="FSMHandle"/> (an FSM instance) is created.
        /// It makes sure the FSM starts in the correct <see cref="InitialState"/>.
        /// This also triggers the `Enter` action defined for that initial state.
        /// </remarks>
        /// <param name="ctx">
        /// The **context** object specific to this FSM instance.
        /// It holds any data or services the state actions might need.
        /// </param>
        /// <exception cref="ArgumentException">
        /// This error occurs if the <see cref="InitialState"/> you set in the FSM blueprint
        /// doesn't actually exist as a defined state. This means your FSM blueprint
        /// has a setup error. An internal error will also be logged.
        /// </exception>
        public void EnterInitial(IStateContext ctx)
        {
            if (!_states.TryGetValue(InitialState, out var state))
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"Initial state '{InitialState}' not found for FSM '{Name}'. This indicates a corrupted FSM definition.",
                    new ArgumentException($"Initial state '{InitialState}' not found for FSM '{Name}'.", nameof(InitialState))
                );
                // Even though we invoke an error, we still return to prevent the FSM from operating in an invalid state.
                return;
            }
            state.Enter(ctx);
        }

        /// <summary>
        /// Makes the FSM instance take a single "step" or "tick,"
        /// which means it evaluates its current situation and potentially
        /// moves to a new state.
        /// </summary>
        /// <remarks>
        /// The FSM processes things in a specific order during each step:
        /// <list type="number">
        ///     <item><description>
        ///         **1. Check "Any State" Transitions:** The FSM first looks at all
        ///         global "Any State" transitions (defined with <see cref="AddAnyStateTransition(string, Func{IStateContext, bool})"/>).
        ///         If any of these conditions are met, the FSM immediately switches
        ///         to that target state, and the `Step` process stops for this tick.
        ///         Any problems during this check (like a condition causing an error,
        ///         or trying to go to a state that doesn't exist) are reported internally,
        ///         but the FSM tries to continue or skip the problematic transition.
        ///     </description></item>
        ///     <item><description>
        ///         **2. Enter State (if needed):** If the current state hasn't been
        ///         officially "entered" yet (meaning its `Enter` action hasn't run),
        ///         it will run now.
        ///     </description></item>
        ///     <item><description>
        ///         **3. Run Current State's Update:** The FSM then executes the
        ///         `Update` action of its current state. If this action causes an
        ///         error, it's caught and reported by <see cref="FSM_API.Error.InvokeInternalApiError(string, Exception)"/>.
        ///     </description></item>
        ///     <item><description>
        ///         **4. Check Regular Transitions:** Finally, the FSM checks all
        ///         transitions that specifically start from the <paramref name="current"/>
        ///         state (defined with <see cref="AddTransition(string, string, Func{IStateContext, bool})"/>).
        ///         The *first* transition whose condition is `true` will cause the
        ///         FSM to move to its target state, and the `Step` process stops.
        ///         Errors here are handled similarly to "Any State" transitions.
        ///     </description></item>
        /// </list>
        /// If the FSM finds itself in a state that isn't defined in its blueprint,
        /// an internal error is logged. As a recovery, it tries to force a transition
        /// back to its <see cref="InitialState"/>.
        /// </remarks>
        /// <param name="current">The name of the state the FSM instance is currently in.</param>
        /// <param name="ctx">The **context** object for this FSM instance.</param>
        /// <param name="next">
        /// This is an **output** that will contain the name of the state the FSM
        /// ends up in after this step. If no transition happens, it will be the
        /// same as <paramref name="current"/>.
        /// </param>
        public void Step(string current, IStateContext ctx, out string next)
        {
            Console.WriteLine("Stepping");
            next = current; // Assume state doesn't change unless a transition fires

            if (!_states.TryGetValue(current, out var currentState))
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"FSM '{Name}' instance in processing group '{ProcessingGroup}' has an invalid current state '{current}'. Attempting to recover by transitioning to initial state '{InitialState}'.", new Exception()
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
                        new Exception()
                    );
                    continue; // Skip this problematic transition
                }

                try
                {
                    if (t.Condition(ctx))
                    {
                        currentState.Exit(ctx);
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

                }
            }


            //// 2. Enter the state if it's unentered
            //if(!ctx.HasEntered)
            //    currentState.Enter(ctx);

            // 3. Execute current state's Update logic
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

            }


            // 4. Check regular transitions from the current state
            foreach (var t in _transitions)
            {
                Console.WriteLine("Here");
                if (t.From == current) // Only consider transitions *from* the current state
                {
                    // Check if the target state exists before evaluating condition
                    if (!_states.ContainsKey(t.To))
                    {
                        FSM_API.Error.InvokeInternalApiError(
                            $"FSM '{Name}' regular transition defined from '{current}' to non-existent state '{t.To}'. Transition skipped for safety.",
                            new Exception()
                        );
                        continue; // Skip this problematic transition
                    }

                    try
                    {
                        Console.WriteLine("Here2");
                        if (t.Condition(ctx))
                        {
                            currentState.Exit(ctx);
                            var handle = FSM_API.Internal.GetFSMHandle(Name, ctx, ProcessingGroup);
                            handle.CurrentState = t.To; // Update the FSM handle's current state
                            handle.HasEnteredCurrentState = false;
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

                    }
                }
            }
        }

        /// <summary>
        /// **Forces** an FSM instance to immediately switch from one state to another,
        /// completely ignoring any normal transition conditions.
        /// </summary>
        /// <remarks>
        /// This method will first run the `Exit` action of the <paramref name="from"/> state
        /// (if that state exists) and then immediately run the `Enter` action of the
        /// <paramref name="to"/> state. It's really useful for taking direct control
        /// or for fixing an FSM that's in a problematic state.
        /// <para>
        /// If there are errors during the state's `Exit` or `Enter` actions, they are
        /// caught and reported using <see cref="FSM_API.Error.InvokeInternalApiError(string, Exception)"/>.
        /// </para>
        /// </remarks>
        /// <param name="from">
        /// The name of the state the FSM is currently in and should exit from.
        /// If this state doesn't exist or is empty, no `Exit` action will be performed,
        /// but the FSM will still try to enter the <paramref name="to"/> state.
        /// </param>
        /// <param name="to">
        /// The name of the state the FSM should enter. This state **must** be
        /// defined in your FSM blueprint.
        /// </param>
        /// <param name="ctx">The **context** object for this FSM instance.</param>
        /// <exception cref="ArgumentException">
        /// This error occurs if the <paramref name="to"/> state you want to force
        /// a transition to does not exist in the FSM blueprint. An internal error
        /// will also be logged.
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
                    null // Passing null for exception as it's not an exception but a missing state
                );
            }

            if (!_states.TryGetValue(to, out var toState))
            {
                FSM_API.Error.InvokeInternalApiError(
                    $"Target state '{to}' for forced transition does not exist in FSM '{Name}'. Forced transition failed.",
                    new ArgumentException($"Target state '{to}' for forced transition does not exist in FSM '{Name}'.", nameof(to))
                );
                return;
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

        /// <summary>
        /// Removes a state with the given name from this FSM blueprint.
        /// </summary>
        /// <remarks>
        /// If you remove a state, any transitions that relied on it (either starting
        /// from it or going to it) will no longer work correctly. It's very important
        /// to make sure no FSM instances are currently in, or trying to move to,
        /// a state that you are removing.
        /// </remarks>
        /// <param name="name">The name of the state you want to remove.</param>
        public void RemoveState(string name)
        {
            _states.Remove(name);
        }

        /// <summary>
        /// Checks if a regular transition exists between the specified starting
        /// and ending states.
        /// </summary>
        /// <param name="fromState">The name of the state where the transition starts.</param>
        /// <param name="toState">The name of the state where the transition ends.</param>
        /// <returns>
        /// <c>true</c> if a transition exists from <paramref name="fromState"/> to
        /// <paramref name="toState"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool HasTransition(string fromState, string toState)
        {
            return _transitions.Any(s => s.From == fromState && s.To == toState);
        }

        /// <summary>
        /// Removes a specific normal transition between two states from the FSM blueprint.
        /// </summary>
        /// <param name="from">The name of the state where the transition starts.</param>
        /// <param name="to">The name of the state where the transition ends.</param>
        public void RemoveTransition(string from, string to)
        {
            if (HasTransition(from, to))
            {
                var fsmTransition = _transitions.First(s => s.From == from && s.To == to);
                _transitions.Remove(fsmTransition);
            }
        }

        /// <summary>
        /// Gets a specific **state object** from the FSM blueprint by its name.
        /// </summary>
        /// <param name="name">The name of the state you want to retrieve.</param>
        /// <returns>
        /// The <see cref="FSMState"/> object if a state with that name is found;
        /// otherwise, <c>null</c>.
        /// </returns>
        public FSMState GetState(string name)
        {
            // Corrected to use TryGetValue for efficiency and proper null handling
            if (_states.TryGetValue(name, out var state))
            {
                return state;
            }
            return null;
        }

        /// <summary>
        /// Gets a specific **normal transition object** based on its starting and ending states.
        /// </summary>
        /// <param name="transition">
        /// A special pair of names where the first item (`Item1`) is the target state
        /// and the second item (`Item2`) is the source state.
        /// </param>
        /// <returns>
        /// The <see cref="FSMTransition"/> object if a matching transition is found;
        /// otherwise, <c>null</c>.
        /// </returns>
        public FSMTransition GetTransition(Tuple<string, string> transition)
        {
            return _transitions.FirstOrDefault(s => s.To == transition.Item1 && s.From == transition.Item2);
        }

        /// <summary>
        ///  Custom string output for presenting the FSM
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"--- FSM Blueprint: {Name} ---");
            sb.AppendLine($"  Initial State: {InitialState}");
            sb.AppendLine($"  Processing Group: {ProcessingGroup}");
            sb.AppendLine($"  Process Rate: {(ProcessRate == -1 ? "Every Tick" : (ProcessRate == 0 ? "Manual" : $"Every {ProcessRate} Ticks"))}");

            sb.AppendLine("\n  States:");
            if (_states.Any())
            {
                foreach (var state in _states.Values)
                {
                    sb.AppendLine($"    - {state.Name}");
                }
            }
            else
            {
                sb.AppendLine("    (No states defined)");
            }

            sb.AppendLine("\n  Transitions:");
            var allTransitions = GetAllTransitions(); // Use the existing method to get all transitions
            if (allTransitions.Any())
            {
                foreach (var transition in allTransitions)
                {
                    if (transition.From == AnyStateIdentifier)
                    {
                        sb.AppendLine($"    - Any State -> {transition.To}");
                    }
                    else
                    {
                        sb.AppendLine($"    - {transition.From} -> {transition.To}");
                    }
                }
            }
            else
            {
                sb.AppendLine("    (No transitions defined)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// This is a special internal text identifier (`"__ANY_STATE__"`)
        /// used to represent "Any State."
        /// </summary>
        /// <remarks>
        /// When you define a transition from "Any State," it means that
        /// transition can be triggered **no matter what state** the FSM
        /// is currently in. It's like a global rule.
        /// This unique text prevents it from clashing with any of your
        /// actual state names.
        /// </remarks>
        public const string AnyStateIdentifier = "__ANY_STATE__"; // Using a unique, unlikely string

        private readonly Dictionary<string, FSMState> _states = new Dictionary<string, FSMState>();
        private readonly List<FSMTransition> _transitions = new List<FSMTransition>(); // Regular transitions
        private readonly List<FSMTransition> _anyStateTransitions = new List<FSMTransition>(); // Any State transitions
    }
}




