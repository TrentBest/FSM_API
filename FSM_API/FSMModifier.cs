
namespace TheSingularityWorkshop.FSM_API
{
    internal class FSMModifier
    {
        private FSM fsm;
        private List<string> _statesToRemove = new List<string>();
        private List<Tuple<string, string>> _transitionsToRemove = new List<Tuple<string, string>>();
        private string _existingProcessGroup = "Update";
        private List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>> _statesToAdd = new List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>>();
        private List<Tuple<string, string, Func<IStateContext, bool>>> _transitionsToAdd = new List<Tuple<string, string, Func<IStateContext, bool>>>();
        private List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>> _statesToModify = new List<Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>>();
        private List<Tuple<string, string, Func<IStateContext, bool>>> _modifiedTransitions = new List<Tuple<string, string, Func<IStateContext, bool>>>();
        private string _modifiedProcessGroup = string.Empty;
        private int _modifiedProcessRate;
        private int _existingProcessRate;

        public FSMModifier(FSM fsm)
        {
            this.fsm = fsm;
        }

        public FSMModifier WithoutState(string stateName)
        {
            if (fsm.HasState(stateName))
            {
                _statesToRemove.Add(stateName);
            }
            return this;
        }

        public FSMModifier WithoutTransition(string fromState, string toState)
        {
            if (fsm.HasTransition(fromState, toState))
            {
                _transitionsToRemove.Add(new Tuple<string, string>(fromState, toState));
            }
            return this;
        }

        public FSMModifier WithState(string stateName, Action<IStateContext> onEnter, Action<IStateContext> onUpdate, Action<IStateContext> onExit)
        {
            if (!fsm.HasState(stateName))
            {
                _statesToAdd.Add(new Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>(stateName, onEnter, onUpdate, onExit));
            }
            return this;
        }

        public FSMModifier WithModifiedState(string stateName, Action<IStateContext> onEnter, Action<IStateContext> onUpdate, Action<IStateContext> onExit)
        {
            if (fsm.HasState(stateName))
            {
                _statesToModify.Add(new Tuple<string, Action<IStateContext>, Action<IStateContext>, Action<IStateContext>>(stateName, onEnter, onUpdate, onExit));
            }
            return this;
        }

        public FSMModifier WithTransition(string fromState, string toState, Func<IStateContext, bool> condition)
        {
            if (!fsm.HasTransition(fromState, toState))
            {
                _transitionsToAdd.Add(new Tuple<string, string, Func<IStateContext, bool>>(fromState, toState, condition));
            }
            return this;
        }

        public FSMModifier WithModifiedTransition(string fromState, string toState, Func<IStateContext, bool> condition)
        {
            if (fsm.HasTransition(fromState, toState))
            {
                _modifiedTransitions.Add(new Tuple<string, string, Func<IStateContext, bool>>(fromState, toState, condition));
            }
            return this;
        }


        public FSMModifier WithProcessGroup(string processGroup)
        {
            _modifiedProcessGroup = processGroup;
            return this;
        }

        public FSMModifier WithProcessRate(int processRate = 0)
        {
            _modifiedProcessRate = processRate;
            return this;
        }

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

                var handlesInState = bucket.Instances.Where(s => s.CurrentState == state).ToList();
                foreach (var handle in handlesInState)
                {
                    handle.TransitionTo(bucket.Definition.InitialState);
                }
                bucket.Definition.RemoveState(state);
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
                var tran = bucket.Definition.GetTranisition(new Tuple<string, string>(transition.Item1, transition.Item2));
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