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
            if (!TryGetState(currentID, out _))
            {
                return currentID;
            }

            for (var i = 0; i < _anyStateTransitions.Count; i++)
            {
                var transition = _anyStateTransitions[i];
                if (!TryGetState(transition.ToID, out _) || !transition.Evaluate(context))
                {
                    continue;
                }

                ForceTransition(currentID, transition.ToID, context);
                return transition.ToID;
            }

            for (var i = 0; i < _transitions.Count; i++)
            {
                var transition = _transitions[i];
                if (transition.FromID != currentID || !TryGetState(transition.ToID, out _) || !transition.Evaluate(context))
                {
                    continue;
                }

                ForceTransition(currentID, transition.ToID, context);
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
            if (!TryGetState(currentID, out var currentState))
            {
                ForceTransition(currentID, InitialStateID, context);
                return InitialStateID;
            }

            currentState.Update(context);

            for (var i = 0; i < _anyStateTransitions.Count; i++)
            {
                var transition = _anyStateTransitions[i];
                if (!TryGetState(transition.ToID, out var nextState) || !transition.Evaluate(context))
                {
                    continue;
                }

                currentState.Exit(context);
                nextState.Enter(context);
                return transition.ToID;
            }

            for (var i = 0; i < _transitions.Count; i++)
            {
                var transition = _transitions[i];
                if (transition.FromID != currentID || !TryGetState(transition.ToID, out var nextState) || !transition.Evaluate(context))
                {
                    continue;
                }

                currentState.Exit(context);
                nextState.Enter(context);
                return transition.ToID;
            }

            return currentID;
        }

        /// <summary>Forces an immediate transition between integer state identities.</summary>
        public void ForceTransition(int fromID, int toID, IStateContext context)
        {
            if (TryGetState(fromID, out var fromState))
            {
                fromState.Exit(context);
            }

            if (!TryGetState(toID, out var toState))
            {
                throw new ArgumentException($"Target state '{toID}' does not exist in FSM '{FSM_ID}'.", nameof(toID));
            }

            toState.Enter(context);
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
