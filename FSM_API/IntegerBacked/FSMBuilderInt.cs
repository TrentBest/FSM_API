using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Fluent builder for integer-backed FSM blueprints.
    /// </summary>
    /// <remarks>
    /// This is the integer counterpart to <see cref="TheSingularityWorkshop.FSM_API.FSMBuilder"/>.
    /// State, FSM, and processing-group identities are supplied as integers so the resulting blueprint
    /// can remain entirely inside the integer-backed representation. No name-to-ID lookup is performed.
    /// </remarks>
    public sealed class FSMBuilderInt
    {
        private readonly int _fsmID;
        private int _processRate;
        private int _processingGroupID;
        private int _initialStateID = FSM.AnyStateIdentifier;
        private readonly List<FSMState> _states = new List<FSMState>();
        private readonly List<FSMTransition> _transitions = new List<FSMTransition>();
        private readonly List<FSMTransition> _anyTransitions = new List<FSMTransition>();

        /// <summary>Creates an integer-backed FSM builder.</summary>
        public FSMBuilderInt(int fsmID, int processRate = 0, int processingGroupID = 0)
        {
            if (fsmID < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fsmID), "FSM IDs must be non-negative array indexes.");
            }

            if (processingGroupID < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(processingGroupID), "Processing-group IDs must be non-negative.");
            }

            _fsmID = fsmID;
            _processRate = processRate;
            _processingGroupID = processingGroupID;
        }

        /// <summary>Adds a state with an integer identity.</summary>
        public FSMBuilderInt State(int stateID, Action<IStateContext> onEnter = null, Action<IStateContext> onUpdate = null, Action<IStateContext> onExit = null)
        {
            ValidateStateID(stateID, nameof(stateID));
            for (var i = 0; i < _states.Count; i++)
            {
                if (_states[i].StateID == stateID)
                {
                    throw new ArgumentException($"State with ID '{stateID}' already exists in this FSM definition.", nameof(stateID));
                }
            }

            _states.Add(new FSMState(stateID, onEnter, onUpdate, onExit));
            return this;
        }

        /// <summary>Sets the automatic processing rate.</summary>
        public FSMBuilderInt WithProcessRate(int rate)
        {
            _processRate = rate;
            return this;
        }

        /// <summary>Sets the integer processing-group identity.</summary>
        public FSMBuilderInt WithProcessingGroup(int processingGroupID)
        {
            if (processingGroupID < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(processingGroupID), "Processing-group IDs must be non-negative.");
            }

            _processingGroupID = processingGroupID;
            return this;
        }

        /// <summary>Sets the integer identity of the initial state.</summary>
        public FSMBuilderInt WithInitialState(int stateID)
        {
            ValidateStateID(stateID, nameof(stateID));
            _initialStateID = stateID;
            return this;
        }

        /// <summary>Adds a normal integer-backed transition.</summary>
        public FSMBuilderInt Transition(int fromID, int toID, Func<IStateContext, bool> condition)
        {
            ValidateStateID(fromID, nameof(fromID));
            ValidateStateID(toID, nameof(toID));
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            _transitions.RemoveAll(t => t.FromID == fromID && t.ToID == toID);
            _transitions.Add(new FSMTransition(fromID, toID, condition));
            return this;
        }

        /// <summary>Adds an integer-backed Any-State transition.</summary>
        public FSMBuilderInt AnyTransition(int toID, Func<IStateContext, bool> condition)
        {
            ValidateStateID(toID, nameof(toID));
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            _anyTransitions.RemoveAll(t => t.ToID == toID);
            _anyTransitions.Add(new FSMTransition(FSM.AnyStateIdentifier, toID, condition));
            return this;
        }

        /// <summary>Builds an independent integer-backed FSM blueprint.</summary>
        public FSM BuildDefinition()
        {
            if (_states.Count == 0)
            {
                throw new InvalidOperationException("An integer-backed FSM must define at least one state.");
            }

            if (_initialStateID != FSM.AnyStateIdentifier && !ContainsState(_initialStateID))
            {
                throw new InvalidOperationException($"Initial state '{_initialStateID}' has not been defined.");
            }

            var definition = new FSM(_fsmID, _processRate)
            {
                ProcessingGroupID = _processingGroupID
            };

            // FSM.AddState establishes the initial state when the first state is added.
            // Add the requested initial state first so no separate name/id registry is required.
            if (_initialStateID != FSM.AnyStateIdentifier)
            {
                definition.AddState(GetState(_initialStateID));
            }

            for (var i = 0; i < _states.Count; i++)
            {
                var state = _states[i];
                if (state.StateID != _initialStateID)
                {
                    definition.AddState(state);
                }
            }

            for (var i = 0; i < _transitions.Count; i++)
            {
                var transition = _transitions[i];
                definition.AddTransition(transition.FromID, transition.ToID, transition.Condition);
            }

            for (var i = 0; i < _anyTransitions.Count; i++)
            {
                var transition = _anyTransitions[i];
                definition.AddAnyStateTransition(transition.ToID, transition.Condition);
            }

            return definition;
        }

        private bool ContainsState(int stateID)
        {
            for (var i = 0; i < _states.Count; i++)
            {
                if (_states[i].StateID == stateID)
                {
                    return true;
                }
            }

            return false;
        }

        private FSMState GetState(int stateID)
        {
            for (var i = 0; i < _states.Count; i++)
            {
                if (_states[i].StateID == stateID)
                {
                    return _states[i];
                }
            }

            throw new InvalidOperationException($"State '{stateID}' has not been defined.");
        }

        private static void ValidateStateID(int stateID, string parameterName)
        {
            if (stateID < 0 || stateID == FSM.AnyStateIdentifier)
            {
                throw new ArgumentOutOfRangeException(parameterName, "State IDs must be non-negative array indexes.");
            }
        }
    }
}
