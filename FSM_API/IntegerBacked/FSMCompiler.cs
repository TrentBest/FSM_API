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

        /// <summary>
        /// Reconstructs a string-backed definition from an integer-backed definition using an explicit
        /// state-ID-to-name mapping. The caller supplies the human-facing FSM and processing-group names.
        /// </summary>
        /// <remarks>
        /// Integer IDs have no intrinsic names, so this conversion never invents state names. Actions and
        /// transition conditions are preserved by delegating to the source integer state and transition.
        /// Runtime instances are not transferred; this operation creates a definition only.
        /// </remarks>
        public static StringFsm Decompile(
            FSM source,
            string fsmName,
            string processingGroup,
            IReadOnlyDictionary<int, string> stateIDToName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("A non-empty FSM name is required.", nameof(fsmName));
            }

            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("A non-empty processing-group name is required.", nameof(processingGroup));
            }

            if (stateIDToName == null)
            {
                throw new ArgumentNullException(nameof(stateIDToName));
            }

            var states = source.GetAllStates().OrderBy(state => state.StateID).ToList();
            if (states.Count == 0)
            {
                throw new InvalidOperationException($"Integer FSM '{source.FSM_ID}' has no states to decompile.");
            }

            var idToName = new Dictionary<int, string>();
            var nameToID = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var state in states)
            {
                if (!stateIDToName.TryGetValue(state.StateID, out var name) || string.IsNullOrWhiteSpace(name))
                {
                    throw new InvalidOperationException($"No valid state name was supplied for integer state ID '{state.StateID}'.");
                }

                if (string.Equals(name, StringFsm.AnyStateIdentifier, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"State name '{name}' is reserved for Any-State transitions.");
                }

                if (nameToID.ContainsKey(name))
                {
                    throw new InvalidOperationException($"State name '{name}' maps to more than one integer state ID.");
                }

                idToName.Add(state.StateID, name);
                nameToID.Add(name, state.StateID);
            }

            if (!idToName.TryGetValue(source.InitialStateID, out var initialStateName))
            {
                throw new InvalidOperationException(
                    $"Initial integer state ID '{source.InitialStateID}' is not defined in the source FSM.");
            }

            var result = new StringFsm
            {
                Name = fsmName,
                ProcessRate = source.ProcessRate,
                ProcessingGroup = processingGroup
            };

            foreach (var state in states)
            {
                var stateName = idToName[state.StateID];
                result.AddState(new StringFsmState(
                    stateName,
                    context => state.Enter(context),
                    context => state.Update(context),
                    context => state.Exit(context)));
            }

            // AddState assigns the first inserted state as initial, so restore the source initial state afterward.
            result.InitialState = initialStateName;

            foreach (var transition in source.GetAllTransitions())
            {
                if (!idToName.TryGetValue(transition.ToID, out var targetName))
                {
                    throw new InvalidOperationException(
                        $"Transition target integer state ID '{transition.ToID}' is not defined in the source FSM.");
                }

                if (transition.FromID == FSM.AnyStateIdentifier)
                {
                    result.AddAnyStateTransition(targetName, transition.Condition);
                    continue;
                }

                if (!idToName.TryGetValue(transition.FromID, out var sourceName))
                {
                    throw new InvalidOperationException(
                        $"Transition source integer state ID '{transition.FromID}' is not defined in the source FSM.");
                }

                result.AddTransition(sourceName, targetName, transition.Condition);
            }

            return result;
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
