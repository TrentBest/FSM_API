using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StringFsm = TheSingularityWorkshop.FSM_API.FSM;
using StringFsmState = TheSingularityWorkshop.FSM_API.FSMState;
using StringFsmTransition = TheSingularityWorkshop.FSM_API.FSMTransition;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Compiles a readable string-backed FSM blueprint into a dense integer-backed definition.
    /// </summary>
    /// <remarks>
    /// State IDs are assigned deterministically by ordinal state-name order. This makes compiled
    /// identities repeatable for the same set of names and keeps the runtime arrays compact. The
    /// source blueprint remains the authoring representation; the compiled definition is the
    /// execution representation.
    /// </remarks>
    public static class FSMCompiler
    {
        /// <summary>
        /// Compiles a string-backed FSM while preserving state actions, transition conditions,
        /// transition order, initial state, and process rate.
        /// </summary>
        /// <param name="source">The authored string-backed FSM blueprint.</param>
        /// <param name="fsmID">The integer identity assigned to the compiled definition.</param>
        /// <param name="processingGroupID">The integer processing-group identity assigned by the caller.</param>
        /// <returns>The integer definition and immutable bidirectional state-name mapping.</returns>
        public static FSMCompilationResult Compile(StringFsm source, int fsmID, int processingGroupID = 0)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var states = source.GetAllStates()
                .OrderBy(state => state.Name, StringComparer.Ordinal)
                .ToList();

            if (states.Count == 0)
            {
                throw new InvalidOperationException($"FSM '{source.Name}' has no states to compile.");
            }

            var nameToID = new Dictionary<string, int>(StringComparer.Ordinal);
            var idToName = new Dictionary<int, string>();
            for (var i = 0; i < states.Count; i++)
            {
                nameToID.Add(states[i].Name, i);
                idToName.Add(i, states[i].Name);
            }

            if (!nameToID.TryGetValue(source.InitialState, out var initialStateID))
            {
                throw new InvalidOperationException(
                    $"FSM '{source.Name}' has initial state '{source.InitialState}', which is not defined.");
            }

            var builder = new FSMBuilderInt(fsmID, source.ProcessRate, processingGroupID)
                .WithInitialState(initialStateID);

            for (var i = 0; i < states.Count; i++)
            {
                StringFsmState state = states[i];
                builder.State(
                    nameToID[state.Name],
                    state.OnEnterAction,
                    state.OnUpdateAction,
                    state.OnExitAction);
            }

            foreach (StringFsmTransition transition in source.GetAllTransitions())
            {
                if (!nameToID.TryGetValue(transition.To, out var toID))
                {
                    throw new InvalidOperationException(
                        $"FSM '{source.Name}' has a transition to undefined state '{transition.To}'.");
                }

                if (string.Equals(transition.From, StringFsm.AnyStateIdentifier, StringComparison.Ordinal))
                {
                    builder.AnyTransition(toID, transition.Condition);
                    continue;
                }

                if (!nameToID.TryGetValue(transition.From, out var fromID))
                {
                    throw new InvalidOperationException(
                        $"FSM '{source.Name}' has a transition from undefined state '{transition.From}'.");
                }

                builder.Transition(fromID, toID, transition.Condition);
            }

            return new FSMCompilationResult(
                builder.BuildDefinition(),
                nameToID,
                idToName);
        }
    }

    /// <summary>
    /// Holds a compiled integer FSM and the immutable mapping used to translate diagnostic names.
    /// </summary>
    public sealed class FSMCompilationResult
    {
        /// <summary>The compiled integer-backed definition.</summary>
        public FSM Definition { get; }

        /// <summary>Maps authored state names to their deterministic integer IDs.</summary>
        public IReadOnlyDictionary<string, int> StateNameToID { get; }

        /// <summary>Maps compiled integer state IDs back to authored names.</summary>
        public IReadOnlyDictionary<int, string> StateIDToName { get; }

        internal FSMCompilationResult(
            FSM definition,
            IDictionary<string, int> stateNameToID,
            IDictionary<int, string> stateIDToName)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            StateNameToID = new ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>(stateNameToID, StringComparer.Ordinal));
            StateIDToName = new ReadOnlyDictionary<int, string>(
                new Dictionary<int, string>(stateIDToName));
        }
    }
}
