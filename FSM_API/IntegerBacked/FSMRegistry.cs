using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Central registry for integer-backed FSM definitions and live instances.
    /// </summary>
    /// <remarks>
    /// This registry intentionally keys definitions and instances by integer identity.
    /// Human-readable names and string-backed lookup belong to the translation boundary,
    /// not to the integer runtime representation.
    /// </remarks>
    public static class FSMRegistry
    {
        private static readonly Dictionary<int, FSM> _definitions = new Dictionary<int, FSM>();
        private static readonly Dictionary<int, List<FSMHandle>> _instances = new Dictionary<int, List<FSMHandle>>();
        private static int _nextHandleID = 1;

        /// <summary>Registers an integer-backed FSM definition.</summary>
        public static void Register(FSM definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (_definitions.ContainsKey(definition.FSM_ID))
            {
                throw new ArgumentException($"An integer FSM with ID '{definition.FSM_ID}' is already registered.", nameof(definition));
            }

            _definitions.Add(definition.FSM_ID, definition);
            _instances.Add(definition.FSM_ID, new List<FSMHandle>());
        }

        /// <summary>Returns whether an integer FSM definition is registered.</summary>
        public static bool Contains(int fsmID)
        {
            return _definitions.ContainsKey(fsmID);
        }

        /// <summary>Gets a registered integer FSM definition, or null when not found.</summary>
        public static FSM Get(int fsmID)
        {
            _definitions.TryGetValue(fsmID, out var definition);
            return definition;
        }

        /// <summary>
        /// Creates a live integer-backed instance from a registered definition.
        /// </summary>
        public static FSMHandle CreateInstance(int fsmID, IStateContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!_definitions.TryGetValue(fsmID, out var definition))
            {
                throw new KeyNotFoundException($"Integer FSM definition '{fsmID}' is not registered.");
            }

            var handle = new FSMHandle(definition, context, _nextHandleID++);
            _instances[fsmID].Add(handle);
            return handle;
        }

        /// <summary>Gets the live instances registered for an integer FSM.</summary>
        public static IReadOnlyList<FSMHandle> GetInstances(int fsmID)
        {
            if (!_instances.TryGetValue(fsmID, out var instances))
            {
                throw new KeyNotFoundException($"Integer FSM definition '{fsmID}' is not registered.");
            }

            return instances.AsReadOnly();
        }

        /// <summary>
        /// Unregisters an integer FSM definition and releases its registry ownership of instances.
        /// Existing handles remain valid as long as external references to them are retained.
        /// </summary>
        public static bool Unregister(int fsmID)
        {
            if (!_definitions.Remove(fsmID))
            {
                return false;
            }

            _instances.Remove(fsmID);
            return true;
        }

        /// <summary>Removes all registered integer FSM definitions and instances from the registry.</summary>
        public static void Clear()
        {
            _definitions.Clear();
            _instances.Clear();
            _nextHandleID = 1;
        }
    }
}
