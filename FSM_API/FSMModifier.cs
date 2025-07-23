
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
            if (!fsm.HasTransition(fromState, toState))
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
            if (_modifiedProcessGroup != _existingProcessGroup)
            {

            }
            else
            {

            }

            if (_modifiedProcessRate != _existingProcessRate)
            {
                bucket.ProcessRate = _modifiedProcessRate;
            }

            if(_statesToRemove.Count > 0)
            {
                foreach (var state in _statesToRemove)
                {
                    bucket.Definition.RemoveState(state);
                    var transitions = bucket.Definition.GetAllTransitions();
                    var anyTrans = transitions.Where(s => s.From == state || s.To == state).ToList();
                    foreach (var trans in anyTrans)
                    {
                        bucket.Definition.RemoveTransition(trans.From, trans.To);
                    }
                }
            }
        }
    }
}