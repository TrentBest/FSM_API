using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Integer-backed FSM blueprint for runtime-oriented state-machine execution.
    /// </summary>
    /// <remarks>
    /// State identities are array indexes. The integer-backed runtime therefore uses direct indexed
    /// access for state lookup instead of hashing or string lookup. The string-backed API remains a
    /// separate authoring/runtime surface; translation between the two representations belongs at
    /// the boundary rather than in this hot-path representation.
    /// </remarks>
    public class FSM
    {
        /// <summary>Unique integer identity of this FSM blueprint.</summary>
        public int FSM_ID { get; }

        /// <summary>Integer identity of the initial state.</summary>
        public int InitialStateID { get; private set; }

        /// <summary>Update frequency. -1 means every tick; 0 means manual; positive values mean every Nth tick.</summary>
        public int ProcessRate { get; internal set; }

        /// <summary>Integer identity of the processing group.</summary>
        public int ProcessingGroupID { get; internal set; }

        /// <summary>Creates an empty integer-backed FSM blueprint.</summary>
        /// <param name="fsmID">Unique integer identity of this FSM.</param>
        /// <param name="processRate">Update frequency: -1 every tick, 0 manual, positive values every Nth tick.</param>
        public FSM(int fsmID = 0, int processRate = 0)
        {
            FSM_ID = fsmID;
            InitialStateID = AnyStateIdentifier;
            ProcessRate = processRate;
        }

        /// <summary>
        /// Adds or replaces a state at its integer identity.
        /// </summary>
        /// <remarks>
        /// State IDs are indexes into the internal state array. IDs must therefore be non-negative and
        /// are intentionally not hashed. Sparse IDs are supported for compatibility with pre-existing
        /// integer definitions, although compiled definitions should normally use dense IDs beginning at zero.
        /// </remarks>
        public void AddState(FSMState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.StateID < 0 || state.StateID == AnyStateIdentifier)
            {
                throw new ArgumentOutOfRangeException(nameof(state), "State IDs must be non-negative array indexes.");
            }

            EnsureStateCapacity(state.StateID + 1);

            var wasEmpty = _states[state.StateID] == null;
            _states[state.StateID] = state;

            if (wasEmpty && InitialStateID == AnyStateIdentifier)
            {
                InitialStateID = state.StateID;
            }
        }

        /// <summary>Adds or replaces a normal transition.</summary>
        public void AddTransition(int fromID, int toID, Func<IStateContext, bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            _transitions.RemoveAll(t => t.FromID == fromID && t.ToID == toID);
            _transitions.Add(new FSMTransition(fromID, toID, condition));
        }

        /// <summary>Adds or replaces an Any-State transition.</summary>
        public void AddAnyStateTransition(int toID, Func<IStateContext, bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            _anyStateTransitions.RemoveAll(t => t.ToID == toID);
            _anyStateTransitions.Add(new FSMTransition(AnyStateIdentifier, toID, condition));
        }

        /// <summary>Checks whether a state exists.</summary>
        public bool HasState(int stateID)
        {
            return TryGetState(stateID, out _);
        }

        /// <summary>Gets a state by integer identity.</summary>
        public FSMState GetState(int stateID)
        {
            return TryGetState(stateID, out var state) ? state : null;
        }

        /// <summary>Returns all states defined by this blueprint.</summary>
        /// <remarks>This is a cold-path inspection operation; runtime state access uses direct array indexing.</remarks>
        public IReadOnlyCollection<FSMState> GetAllStates()
        {
            var states = new List<FSMState>();
            for (var i = 0; i < _states.Length; i++)
            {
                if (_states[i] != null)
                {
                    states.Add(_states[i]);
                }
            }

            return states.AsReadOnly();
        }

        /// <summary>Returns all regular and Any-State transitions.</summary>
        public IReadOnlyCollection<FSMTransition> GetAllTransitions()
        {
            var transitions = new List<FSMTransition>(_anyStateTransitions);
            transitions.AddRange(_transitions);
            return transitions.AsReadOnly();
        }

        /// <summary>Checks whether a transition exists.</summary>
        public bool HasTransition(int fromID, int toID)
        {
            if (fromID == AnyStateIdentifier)
            {
                for (var i = 0; i < _anyStateTransitions.Count; i++)
                {
                    if (_anyStateTransitions[i].ToID == toID)
                    {
                        return true;
                    }
                }

                return false;
            }

            for (var i = 0; i < _transitions.Count; i++)
            {
                if (_transitions[i].FromID == fromID && _transitions[i].ToID == toID)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Removes a state and transitions connected to it.</summary>
        public void RemoveState(int stateID)
        {
            if (!TryGetState(stateID, out _))
            {
                return;
            }

            _states[stateID] = null;
            _transitions.RemoveAll(t => t.FromID == stateID || t.ToID == stateID);
            _anyStateTransitions.RemoveAll(t => t.ToID == stateID);

            if (InitialStateID == stateID)
            {
                InitialStateID = AnyStateIdentifier;
                for (var i = 0; i < _states.Length; i++)
                {
                    if (_states[i] != null)
                    {
                        InitialStateID = i;
                        break;
                    }
                }
            }
        }

        /// <summary>Removes a normal transition.</summary>
        public void RemoveTransition(int fromID, int toID)
        {
            _transitions.RemoveAll(t => t.FromID == fromID && t.ToID == toID);
        }

        /// <summary>Enters the initial state.</summary>
        public void EnterInitial(IStateContext context)
        {
            if (!TryGetState(InitialStateID, out var state))
            {
                throw new InvalidOperationException($"Initial state '{InitialStateID}' does not exist in FSM '{FSM_ID}'.");
            }

            state.Enter(context);
        }

        /// <summary>
        /// Evaluates transitions for the current state without running its Update action.
        /// </summary>
        /// <remarks>
        /// This is the integer-backed counterpart to the string API's manual condition evaluation.
        /// Any-State transitions are considered first, followed by regular transitions. At most one
        /// transition is taken during an evaluation.
        /// </remarks>
        public int EvaluateConditions(int currentID, IStateContext context)
        {
            return EvaluateConditions(currentID, context, out _, out _, null);
        }

        /// <summary>
        /// Evaluates transitions and reports whether a transition completed, including a same-state transition.
        /// </summary>
        public int EvaluateConditions(int currentID, IStateContext context, out bool transitioned)
        {
            return EvaluateConditions(currentID, context, out transitioned, out _, null);
        }

        /// <summary>
        /// Evaluates transitions and reports whether the current state's Exit callback ran
        /// without a target being entered.
        /// </summary>
        public int EvaluateConditions(int currentID, IStateContext context, out bool transitioned, out bool currentStateExited)
        {
            return EvaluateConditions(currentID, context, out transitioned, out currentStateExited, null);
        }

        internal int EvaluateConditions(int currentID, IStateContext context, out bool transitioned, out bool currentStateExited, FSMHandle owner)
        {
            transitioned = false;
            currentStateExited = false;
            // Any-State transitions remain applicable even if a live handle's current
            // state was removed from this runtime-editable definition. In that case there
            // is no source Exit callback, but a valid Any-State target can still recover
            // the handle. Regular transitions naturally remain inapplicable without a
            // matching source state.
            var anyStateTransitions = _anyStateTransitions.ToArray();
            for (var i = 0; i < anyStateTransitions.Length; i++)
            {
                var transition = anyStateTransitions[i];
                var conditionMet = TryGetState(transition.ToID, out _) && EvaluateCondition(transition, currentID, context);
                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)) return currentID;
                if (!context.IsValid) return currentID;
                if (!conditionMet)
                {
                    continue;
                }

                // Guards are user code and may unregister the owning definition.
                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)) return currentID;
                if (!TryGetState(transition.ToID, out _)) continue;

                var transitionCompleted = ForceTransition(currentID, transition.ToID, context, owner);
                if (!transitionCompleted)
                {
                    currentStateExited = owner != null && !owner.HasEnteredCurrentState;
                    return currentID;
                }
                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction))
                {
                    currentStateExited = true;
                    return currentID;
                }

                if (!TryGetState(transition.ToID, out _))
                {
                    currentStateExited = true;
                    return currentID;
                }

                transitioned = true;
                return transition.ToID;
            }

            var transitions = _transitions.ToArray();
            for (var i = 0; i < transitions.Length; i++)
            {
                var transition = transitions[i];
                if (transition.FromID != currentID) continue;
                var conditionMet = TryGetState(transition.ToID, out _) && EvaluateCondition(transition, currentID, context);
                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)) return currentID;
                if (!context.IsValid) return currentID;
                if (!conditionMet) continue;

                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)) return currentID;
                if (!TryGetState(transition.ToID, out _)) continue;

                var transitionCompleted = ForceTransition(currentID, transition.ToID, context, owner);
                if (!transitionCompleted)
                {
                    currentStateExited = owner != null && !owner.HasEnteredCurrentState;
                    return currentID;
                }
                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction))
                {
                    currentStateExited = true;
                    return currentID;
                }

                if (!TryGetState(transition.ToID, out _))
                {
                    currentStateExited = true;
                    return currentID;
                }

                transitioned = true;
                return transition.ToID;
            }

            return currentID;
        }

        /// <summary>
        /// Executes one FSM step and returns the resulting current state identity.
        /// </summary>
        /// <remarks>
        /// The current state is retrieved by direct array indexing. No string state identity is created,
        /// compared, or resolved by this execution path.
        /// </remarks>
        public int Step(int currentID, IStateContext context)
        {
            return StepCore(currentID, context, null);
        }

        internal int Step(int currentID, IStateContext context, FSMHandle owner)
        {
            return StepCore(currentID, context, owner);
        }

        private int StepCore(int currentID, IStateContext context, FSMHandle owner)
        {
            if (!TryGetState(currentID, out var currentState))
            {
                TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                    $"FSM '{FSM_ID}' has invalid current state '{currentID}'. Attempting recovery to initial state '{InitialStateID}'.",
                    new InvalidOperationException($"Current state '{currentID}' does not exist in FSM '{FSM_ID}'."));

                // Match the string-backed recovery contract: an invalid current state
                // is recovered to the configured initial identity. If that target was
                // also removed, report the broken definition without throwing from Step.
                if (!TryGetState(InitialStateID, out _))
                {
                    TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                        $"Initial state '{InitialStateID}' does not exist in integer FSM '{FSM_ID}'. Recovery cannot enter a state.",
                        new InvalidOperationException($"Initial state '{InitialStateID}' does not exist in FSM '{FSM_ID}'."));
                    return InitialStateID;
                }

                return ForceTransition(currentID, InitialStateID, context, owner)
                    ? InitialStateID
                    : currentID;
            }

            try
            {
                currentState.Update(context);
            }
            catch (Exception ex)
            {
                TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                    $"Error during Update logic of integer state '{currentID}' in FSM '{FSM_ID}'. Exception: {ex.Message}",
                    ex);
            }

            // A callback may unregister the definition and destroy this handle.
            // Stop the step before evaluating guards or invoking further callbacks.
            if ((owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)) || !context.IsValid)
            {
                return currentID;
            }

            // Snapshot collections so callbacks can safely add/remove transitions while stepping.
            var anyStateTransitions = _anyStateTransitions.ToArray();
            for (var i = 0; i < anyStateTransitions.Length; i++)
            {
                var transition = anyStateTransitions[i];
                if (!TryGetState(transition.ToID, out _))
                {
                    continue;
                }

                var conditionMet = EvaluateStepCondition(transition, currentID, context);
                if ((owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)) || !context.IsValid)
                {
                    return currentID;
                }

                if (!conditionMet || !TryGetState(transition.ToID, out _))
                {
                    continue;
                }

                try
                {
                    if (owner != null) owner.ExitState(currentState); else currentState.Exit(context);
                }
                catch (Exception ex)
                {
                    TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                        $"Error during Exit logic of integer state '{currentID}' in FSM '{FSM_ID}'. Exception: {ex.Message}",
                        ex);
                    if (!context.IsValid)
                    {
                        if (owner != null) owner.HasEnteredCurrentState = false;
                        return currentID;
                    }
                    continue;
                }

                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction))
                {
                    return currentID;
                }
                if (!context.IsValid)
                {
                    if (owner != null) owner.HasEnteredCurrentState = false;
                    return currentID;
                }

                return TryGetState(transition.ToID, out _) ? transition.ToID : currentID;
            }

            var transitions = _transitions.ToArray();
            for (var i = 0; i < transitions.Length; i++)
            {
                var transition = transitions[i];
                if (transition.FromID != currentID || !TryGetState(transition.ToID, out _))
                {
                    continue;
                }

                var conditionMet = EvaluateStepCondition(transition, currentID, context);
                if ((owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)) || !context.IsValid)
                {
                    return currentID;
                }

                if (!conditionMet || !TryGetState(transition.ToID, out _))
                {
                    continue;
                }

                try
                {
                    if (owner != null) owner.ExitState(currentState); else currentState.Exit(context);
                }
                catch (Exception ex)
                {
                    TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                        $"Error during Exit logic of integer state '{currentID}' in FSM '{FSM_ID}'. Exception: {ex.Message}",
                        ex);
                    if (!context.IsValid)
                    {
                        if (owner != null) owner.HasEnteredCurrentState = false;
                        return currentID;
                    }
                    continue;
                }

                if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction))
                {
                    return currentID;
                }
                if (!context.IsValid)
                {
                    if (owner != null) owner.HasEnteredCurrentState = false;
                    return currentID;
                }

                return TryGetState(transition.ToID, out _) ? transition.ToID : currentID;
            }

            return currentID;
        }

        /// <summary>Forces an immediate transition between integer state identities.</summary>
        public void ForceTransition(int fromID, int toID, IStateContext context)
        {
            ForceTransition(fromID, toID, context, null);
        }

        internal bool ForceTransition(int fromID, int toID, IStateContext context, FSMHandle owner)
        {
            if (context == null || !context.IsValid ||
                (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction)))
            {
                return false;
            }

            var exitStarted = false;
            if (TryGetState(fromID, out var fromState))
            {
                exitStarted = true;
                try
                {
                    if (owner != null) owner.ExitState(fromState); else fromState.Exit(context);
                }
                catch (Exception ex)
                {
                    TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                        $"Error during Exit logic of integer state '{fromID}' during forced transition to '{toID}' in FSM '{FSM_ID}'. Exception: {ex.Message}",
                        ex);
                    // Match the string-backed contract: an Exit failure is reported,
                    // but does not prevent the target state's Enter callback unless
                    // that callback also closed the execution gate.
                    if (!context.IsValid)
                    {
                        if (owner != null) owner.HasEnteredCurrentState = false;
                        return false;
                    }
                }

                if (!context.IsValid)
                {
                    if (owner != null) owner.HasEnteredCurrentState = false;
                    return false;
                }
            }

            if (owner != null && (owner.IsDestroyed || owner.IsPendingDestruction))
            {
                return false;
            }

            if (!TryGetState(toID, out var toState))
            {
                TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                    $"Target state '{toID}' for forced transition does not exist in integer FSM '{FSM_ID}'. Forced transition failed.",
                    new ArgumentException($"Target state '{toID}' does not exist in FSM '{FSM_ID}.", nameof(toID)));
                if (owner != null && exitStarted) owner.HasEnteredCurrentState = false;
                return false;
            }

            // Commit the target identity before invoking user code. A target Enter
            // callback can unregister this definition, which tears down the handle
            // synchronously; teardown must Exit the target being entered rather than
            // invoke Exit on the source state a second time.
            if (owner != null)
            {
                owner.CurrentStateID = toID;
                owner.HasEnteredCurrentState = false;
            }

            try
            {
                toState.Enter(context);
            }
            catch (Exception ex)
            {
                TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                    $"Error during Enter logic of integer state '{toID}' during forced transition from '{fromID}' in FSM '{FSM_ID}'. Exception: {ex.Message}",
                    ex);

                // The target identity is committed before Enter so that re-entrant
                // teardown exits the state whose Enter callback is running. If Enter
                // fails without destroying the handle, restore the source identity;
                // the source has already exited and therefore is no longer entered.
                if (owner != null && !owner.IsDestroyed && !owner.IsPendingDestruction)
                {
                    owner.CurrentStateID = fromID;
                    owner.HasEnteredCurrentState = false;
                }

                throw;
            }

            if (owner != null && !owner.IsDestroyed && !owner.IsPendingDestruction)
            {
                owner.HasEnteredCurrentState = true;
            }
            return owner == null || (!owner.IsDestroyed && !owner.IsPendingDestruction);
        }

        private bool EvaluateStepCondition(FSMTransition transition, int currentID, IStateContext context)
        {
            try
            {
                return transition.Evaluate(context);
            }
            catch (Exception ex)
            {
                TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                    $"Error evaluating integer transition condition from '{currentID}' to '{transition.ToID}' in FSM '{FSM_ID}'. Exception: {ex.Message}",
                    ex);
                return false;
            }
        }

        private bool EvaluateCondition(FSMTransition transition, int currentID, IStateContext context)
        {
            try
            {
                return transition.Evaluate(context);
            }
            catch (Exception ex)
            {
                // Match string-backed manual evaluation: isolate a failing guard to its
                // candidate so later transitions can still be considered.
                TheSingularityWorkshop.FSM_API.FSM_API.Error.InvokeInternalApiError(
                    $"Error evaluating manual integer transition condition from '{currentID}' to '{transition.ToID}' in FSM '{FSM_ID}'. Exception: {ex.Message}",
                    ex);
                return false;
            }
        }

        /// <summary>Reserved integer identity representing Any-State.</summary>
        public const int AnyStateIdentifier = int.MinValue;

        private FSMState[] _states = new FSMState[0];
        private readonly List<FSMTransition> _transitions = new List<FSMTransition>();
        private readonly List<FSMTransition> _anyStateTransitions = new List<FSMTransition>();

        private bool TryGetState(int stateID, out FSMState state)
        {
            if (stateID >= 0 && stateID < _states.Length)
            {
                state = _states[stateID];
                return state != null;
            }

            state = null;
            return false;
        }

        private void EnsureStateCapacity(int requiredLength)
        {
            if (requiredLength <= _states.Length)
            {
                return;
            }

            var newLength = _states.Length == 0 ? 4 : _states.Length;
            while (newLength < requiredLength)
            {
                newLength *= 2;
            }

            Array.Resize(ref _states, newLength);
        }
    }
}
