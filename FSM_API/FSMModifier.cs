
namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Provides a fluent API for dynamically modifying an existing Finite State Machine (FSM) definition at runtime.
    /// </summary>
    /// <remarks>
    /// This class allows for deferred modifications to an FSM's states, transitions,
    /// processing group, or processing rate. Changes are staged using the <c>With</c>
    /// and <c>Without</c> methods and are only applied when <see cref="ModifyDefinition"/> is called.
    /// <para>
    /// Using this modifier ensures that all necessary internal adjustments are made when
    /// an FSM definition changes, including handling instances that might be in a state
    /// that is being removed.
    /// </para>
    /// </remarks>
    internal class FSMModifier
    {
        private FSM fsm;
        private List<Tuple<string,string>> _statesToRemove = new List<Tuple<string, string>>();
        private List<Tuple<string, string>> _transitionsToRemove = new List<Tuple<string, string>>();
        private string _existingProcessGroup = "Update";
        private string _existingFSMName;
        private List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>> _statesToAdd = new List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>>();
        private List<Tuple<string, string, Func<IStateContext, bool>>> _transitionsToAdd = new List<Tuple<string, string, Func<IStateContext, bool>>>();
        private List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>> _statesToModify = new List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>>();
        private List<Tuple<string, string, Func<IStateContext, bool>>> _modifiedTransitions = new List<Tuple<string, string, Func<IStateContext, bool>>>();
        private string _modifiedProcessGroup = string.Empty;
        private int _modifiedProcessRate;
        private int _existingProcessRate = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="FSMModifier"/> class for a specific FSM definition.
        /// </summary>
        /// <param name="fsm">The <see cref="FSM"/> definition to be modified.</param>

        public FSMModifier(FSM fsm)
        {
            this.fsm = fsm;
            _existingProcessRate = fsm.ProcessRate;
            _existingProcessGroup = fsm.ProcessingGroup;
            _existingFSMName = fsm.Name;
        }


        /// <summary>
        /// Stages a state for removal from the FSM definition.
        /// </summary>
        /// <remarks>
        /// When <see cref="ModifyDefinition"/> is called, if any active FSM instances
        /// are currently in the state being removed, they will be transitioned to the
        /// specified <paramref name="fallbackStateName"/>.
        /// <para>
        /// **Important:** If <paramref name="fallbackStateName"/> is null, empty, or consists only of whitespace,
        /// instances will instead transition to the FSM's <see cref="FSM.InitialState"/>.
        /// It is recommended to provide a valid, existing fallback state to ensure predictable behavior.
        /// </para>
        /// If the <paramref name="stateName"/> to remove does not exist in the FSM, this staging
        /// operation has no effect.
        /// </remarks>
        /// <param name="stateName">The name of the state to remove.</param>
        /// <param name="fallbackStateName">
        /// The name of the state to transition existing FSM instances to if they are
        /// currently in the state being removed. If not provided (null or empty), instances
        /// will default to transitioning to the FSM's <see cref="FSM.InitialState"/>.
        /// </param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>


        public FSMModifier WithoutState(string stateName, string fallbackStateName)
        {
            if (fsm.HasState(stateName))
            {
                _statesToRemove.Add(new Tuple<string, string>(stateName, fallbackStateName));
            }
            return this;
        }


        /// <summary>
        /// Stages a specific transition for removal from the FSM definition.
        /// </summary>
        /// <remarks>
        /// This method targets a transition by its source and destination states.
        /// If the transition does not exist in the FSM, this operation has no effect.
        /// </remarks>
        /// <param name="fromState">The name of the state from which the transition originates.</param>
        /// <param name="toState">The name of the state to which the transition leads.</param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>

        public FSMModifier WithoutTransition(string fromState, string toState)
        {
            if (fsm.HasTransition(fromState, toState))
            {
                _transitionsToRemove.Add(new Tuple<string, string>(fromState, toState));
            }
            return this;
        }


        /// <summary>
        /// Stages a new state for addition to the FSM definition.
        /// </summary>
        /// <remarks>
        /// If a state with the given <paramref name="stateName"/> already exists, this operation
        /// will be ignored. To modify an existing state's actions, use <see cref="WithModifiedState"/>.
        /// </remarks>
        /// <param name="stateName">The unique name of the state to add.</param>
        /// <param name="onEnter">An <see cref="Action{IStateContext}"/> to execute when entering this state. Can be <c>null</c>.</param>
        /// <param name="onUpdate">An <see cref="Action{IStateContext}"/> to execute each tick while in this state. Can be <c>null</c>.</param>
        /// <param name="onExit">An <see cref="Action{IStateContext}"/> to execute when exiting this state. Can be <c>null</c>.</param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>

        public FSMModifier WithState(string stateName, Action<IStateContext> onEnter, Action<IStateContext> onUpdate, Action<IStateContext> onExit)
        {
            if (!fsm.HasState(stateName))
            {
                _statesToAdd.Add(new Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>(stateName, onEnter, onUpdate, onExit));
            }
            return this;
        }


        /// <summary>
        /// Stages an existing state's actions for modification.
        /// </summary>
        /// <remarks>
        /// This method allows updating the <c>onEnter</c>, <c>onUpdate</c>, and <c>onExit</c>
        /// actions of a state that is already part of the FSM definition.
        /// If the state does not exist, this operation has no effect.
        /// </remarks>
        /// <param name="stateName">The name of the state to modify.</param>
        /// <param name="onEnter">The new <see cref="Action{IStateContext}"/> for when entering this state. Can be <c>null</c>.</param>
        /// <param name="onUpdate">The new <see cref="Action{IStateContext}"/> for when updating in this state. Can be <c>null</c>.</param>
        /// <param name="onExit">The new <see cref="Action{IStateContext}"/> for when exiting this state. Can be <c>null</c>.</param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>

        public FSMModifier WithModifiedState(string stateName, Action<IStateContext> onEnter, Action<IStateContext> onUpdate, Action<IStateContext> onExit)
        {
            if (fsm.HasState(stateName))
            {
                _statesToModify.Add(new Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>(stateName, onEnter, onUpdate, onExit));
            }
            return this;
        }


        /// <summary>
        /// Stages a new transition for addition to the FSM definition.
        /// </summary>
        /// <remarks>
        /// If a transition from <paramref name="fromState"/> to <paramref name="toState"/>
        /// already exists, this operation will be ignored. To modify an existing transition's
        /// condition, use <see cref="WithModifiedTransition"/>.
        /// </remarks>
        /// <param name="fromState">The name of the state from which the transition originates.</param>
        /// <param name="toState">The name of the state to which the transition leads.</param>
        /// <param name="condition">A <see cref="Func{IStateContext, Boolean}"/> that returns <c>true</c> when the transition should occur.</param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>

        public FSMModifier WithTransition(string fromState, string toState, Func<IStateContext, bool> condition)
        {
            if (!fsm.HasTransition(fromState, toState))
            {
                _transitionsToAdd.Add(new Tuple<string, string, Func<IStateContext, bool>>(fromState, toState, condition));
            }
            return this;
        }


        /// <summary>
        /// Stages an existing transition's condition for modification.
        /// </summary>
        /// <remarks>
        /// This method allows updating the transition condition between two existing states.
        /// If the transition does not exist, this operation has no effect.
        /// </remarks>
        /// <param name="fromState">The name of the state from which the transition originates.</param>
        /// <param name="toState">The name of the state to which the transition leads.</param>
        /// <param name="condition">The new <see cref="Func{IStateContext, Boolean}"/> that returns <c>true</c> when the transition should occur.</param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>

        public FSMModifier WithModifiedTransition(string fromState, string toState, Func<IStateContext, bool> condition)
        {
            if (fsm.HasTransition(fromState, toState))
            {
                _modifiedTransitions.Add(new Tuple<string, string, Func<IStateContext, bool>>(fromState, toState, condition));
            }
            return this;
        }


        /// <summary>
        /// Stages a change to the FSM's processing group.
        /// </summary>
        /// <remarks>
        /// When <see cref="ModifyDefinition"/> is called, the FSM and all its instances
        /// will be moved from their current processing group to the specified new group.
        /// </remarks>
        /// <param name="processGroup">The new name of the processing group for this FSM.</param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>

        public FSMModifier WithProcessGroup(string processGroup)
        {
            _modifiedProcessGroup = processGroup;
            return this;
        }

        /// <summary>
        /// Stages a change to the FSM's processing rate.
        /// </summary>
        /// <remarks>
        /// This sets how frequently the FSM instances associated with this definition
        /// will be updated when their processing group is ticked by the <c>FSM_API</c>'s
        /// internal management system (e.g., <c>FSM_API.Interaction.TickAll</c>).
        /// <list type="bullet">
        /// <item><c>0</c>: **Event-Driven / Manual Processing.** FSM instances will NOT be automatically
        /// updated by <c>FSM_API.Interaction.TickAll</c>. Instead, their <see cref="FSMHandle.Update"/>
        /// or <see cref="FSMHandle.EvaluateConditions"/> methods must be explicitly called by the user's application.</item>
        /// <item><c>-1</c>: **Process Every Tick.** FSM instances will be processed on every invocation
        /// of <c>FSM_API.Interaction.TickAll</c> for their processing group.</item>
        /// <item><c>&gt; 0</c>: **Process Every N Ticks (Skipping N-1).** FSM instances will be processed
        /// every <c>N</c> ticks. For example, if <c>processRate</c> is <c>2</c>, instances will be processed
        /// every second tick (skipping 1 tick in between). If <c>processRate</c> is <c>3</c>, they will be processed
        /// every third tick (skipping 2 ticks in between).</item>
        /// </list>
        /// </remarks>
        /// <param name="processRate">The new processing rate. Defaults to <c>0</c>.</param>
        /// <returns>The current <see cref="FSMModifier"/> instance for fluent chaining.</returns>


        public FSMModifier WithProcessRate(int processRate = 0)
        {
            _modifiedProcessRate = processRate;
            return this;
        }


        /// <summary>
        /// Applies all staged modifications to the FSM definition.
        /// </summary>
        /// <remarks>
        /// This method executes all the changes specified by the preceding <c>With</c> and <c>Without</c>
        /// calls. If the FSM definition or its associated bucket cannot be found, the method will
        /// gracefully exit without applying changes.
        /// <para>
        /// **Important Considerations:**
        /// <list type="bullet">
        /// <item>
        /// **State Removal:** If a state is removed, any active <see cref="FSMHandle"/> instances
        /// currently in that state will be immediately transitioned to the FSM's initial state.
        /// The <c>OnExit</c> action of the removed state will be executed for these instances.
        /// </item>
        /// <item>
        /// **Process Group/Rate:** Changes to the process group or rate apply to the entire FSM definition
        /// and all its instances.
        /// </item>
        /// <item>
        /// **Atomicity:** While modifications are batched, the actual application occurs sequentially.
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        public void ModifyDefinition()
        {
            var bucket = FSM_API.Internal.GetBucket(fsm.Name, _existingProcessGroup);
            if (bucket == null || bucket.Definition == null)
            {
                return;
            }
            if (_modifiedProcessGroup != _existingProcessGroup)
            {
                FSM_API.Internal.SetProcessGroup(_modifiedProcessGroup, bucket);
            }
            if (_modifiedProcessRate != _existingProcessRate)
            {
                bucket.ProcessRate = _modifiedProcessRate;
            }

            foreach (var state in _statesToRemove)
            {

                var handlesInState = bucket.Instances.Where(s => s.CurrentState == state.Item1).ToList();
                foreach (var handle in handlesInState)
                {
                    handle.TransitionTo(bucket.Definition.InitialState);
                }
                bucket.Definition.RemoveState(state.Item1);
            }


            foreach (var state in _statesToAdd)
            {
                bucket.Definition.AddState(new FSMState(state.Item1, state.Item2, state.Item3, state.Item4));
            }


            foreach (var state in _statesToModify)
            {
                var stateData = bucket.Definition.GetState(state.Item1);
                if (stateData != null)
                {
                    if (stateData.Enter != state.Item2)
                    {
                        stateData.SetOnEnter(state.Item2);
                    }
                    if (stateData.Update != state.Item3)
                    {
                        stateData.SetOnUpdate(state.Item3);
                    }
                    if (stateData.Exit != state.Item4)
                    {
                        stateData.SetOnExit(state.Item4);
                    }
                }
            }


            foreach (var transition in _transitionsToRemove)
            {
                bucket.Definition.RemoveTransition(transition.Item1, transition.Item2);
            }

            foreach (var transition in _transitionsToAdd)
            {
                bucket.Definition.AddTransition(transition.Item1, transition.Item2, transition.Item3);
            }

            foreach (var transition in _modifiedTransitions)
            {
                var tran = bucket.Definition.GetTransition(new Tuple<string, string>(transition.Item1, transition.Item2));
                if (tran == null)
                {
                    continue;
                }
                if (tran.From != transition.Item1)
                {
                    tran.From = transition.Item1;
                }
                if (tran.To != transition.Item2)
                {
                    tran.To = transition.Item2;
                }
                if (tran.Condition != transition.Item3)
                {
                    tran.Condition = transition.Item3;
                }
            }


        }
    }
}