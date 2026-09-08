using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Integer-backed FSM blueprint for runtime-oriented state-machine execution.
    /// </summary>
    /// <remarks>
    /// This is the first composite layer built from the integer-backed atoms.
    /// State identity and transition endpoints are represented directly by integers;
    /// no string lookup is required by the state-machine definition itself.
    /// </remarks>
    public class FSM
    {
        /// <summary>Unique integer identity of this FSM blueprint.</summary>
        public int FSM_ID { get; }

        /// <summary>Integer identity of the initial state.</summary>
        public int InitialStateID { get; private set; }

        /// <summary>Update frequency. -1 means every tick; 0 means manual.</summary>
        public int ProcessRate { get; internal set; }

        /// <summary>Integer identity of the processing group.</summary>
        public int ProcessingGroupID { get; internal set; }

        /// <summary>Creates an empty integer-backed FSM blueprint.</summary>
        public FSM(int fsmID = 0)
        {
            FSM_ID = fsmID;
            InitialStateID = AnyStateIdentifier;
            ProcessRate = 0;
        }

        /// <summary>Adds or replaces a state by its integer identity.</summary>
        public void AddState(FSMState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _states[state.StateID] = state;
            if (_states.Count == 1)
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
            return _states.ContainsKey(stateID);
        }

        /// <summary>Gets a state by integer identity.</summary>
        public FSMState GetState(int stateID)
        {
            _states.TryGetValue(stateID, out var state);
            return state;
        }

        /// <summary>Returns all states defined by this blueprint.</summary>
        public IReadOnlyCollection<FSMState> GetAllStates()
        {
            return _states.Values.ToList().AsReadOnly();
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
                return _anyStateTransitions.Any(t => t.ToID == toID);
            }

            return _transitions.Any(t => t.FromID == fromID && t.ToID == toID);
        }

        /// <summary>Removes a state and transitions connected to it.</summary>
        public void RemoveState(int stateID)
        {
            if (!_states.Remove(stateID))
            {
                return;
            }

            _transitions.RemoveAll(t => t.FromID == stateID || t.ToID == stateID);
            _anyStateTransitions.RemoveAll(t => t.ToID == stateID);

            if (InitialStateID == stateID)
            {
                InitialStateID = AnyStateIdentifier;
                foreach (var remainingStateID in _states.Keys)
                {
                    InitialStateID = remainingStateID;
                    break;
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
            var state = GetState(InitialStateID);
            if (state == null)
            {
                throw new InvalidOperationException($"Initial state '{InitialStateID}' does not exist in FSM '{FSM_ID}'.");
            }

            state.Enter(context);
        }

        /// <summary>
        /// Executes one FSM step and returns the resulting current state identity.
        /// </summary>
        public int Step(int currentID, IStateContext context)
        {
            if (!_states.TryGetValue(currentID, out var currentState))
            {
                ForceTransition(currentID, InitialStateID, context);
                return InitialStateID;
            }

            currentState.Update(context);

            foreach (var transition in _anyStateTransitions)
            {
                if (!_states.ContainsKey(transition.ToID))
                {
                    continue;
                }

                if (transition.Evaluate(context))
                {
                    currentState.Exit(context);
                    var nextState = _states[transition.ToID];
                    nextState.Enter(context);
                    return transition.ToID;
                }
            }

            foreach (var transition in _transitions)
            {
                if (transition.FromID != currentID || !_states.ContainsKey(transition.ToID))
                {
                    continue;
                }

                if (transition.Evaluate(context))
                {
                    currentState.Exit(context);
                    var nextState = _states[transition.ToID];
                    nextState.Enter(context);
                    return transition.ToID;
                }
            }

            return currentID;
        }

        /// <summary>Forces an immediate transition between integer state identities.</summary>
        public void ForceTransition(int fromID, int toID, IStateContext context)
        {
            if (_states.TryGetValue(fromID, out var fromState))
            {
                fromState.Exit(context);
            }

            if (!_states.TryGetValue(toID, out var toState))
            {
                throw new ArgumentException($"Target state '{toID}' does not exist in FSM '{FSM_ID}'.", nameof(toID));
            }

            toState.Enter(context);
        }

        /// <summary>Reserved integer identity representing Any-State.</summary>
        public const int AnyStateIdentifier = int.MinValue;

        private readonly Dictionary<int, FSMState> _states = new Dictionary<int, FSMState>();
        private readonly List<FSMTransition> _transitions = new List<FSMTransition>();
        private readonly List<FSMTransition> _anyStateTransitions = new List<FSMTransition>();
    }
}
