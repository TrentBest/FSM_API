using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Owns integer-backed FSM definitions and their live handles.
    /// </summary>
    /// <remarks>
    /// This is the runtime orchestration layer above <see cref="FSM"/> and
    /// <see cref="FSMHandle"/>. It deliberately identifies definitions and
    /// processing groups by integer IDs so the runtime path does not require
    /// the string-backed registry.
    /// </remarks>
    public sealed class FSMRuntime
    {
        private readonly Dictionary<int, FSM> _definitions = new Dictionary<int, FSM>();
        private readonly List<FSMHandle> _handles = new List<FSMHandle>();
        private int _nextHandleID;

        /// <summary>Registers an integer-backed FSM definition for a processing group.</summary>
        public void Register(FSM definition, int processingGroupID = 0)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            definition.ProcessingGroupID = processingGroupID;
            _definitions[definition.FSM_ID] = definition;
        }

        /// <summary>Returns whether an FSM definition is registered.</summary>
        public bool Contains(int fsmID)
        {
            return _definitions.ContainsKey(fsmID);
        }

        /// <summary>Gets a registered FSM definition by integer identity.</summary>
        public FSM GetDefinition(int fsmID)
        {
            _definitions.TryGetValue(fsmID, out var definition);
            return definition;
        }

        /// <summary>Creates a live handle for a registered FSM definition.</summary>
        public FSMHandle CreateInstance(int fsmID, IStateContext context)
        {
            if (!_definitions.TryGetValue(fsmID, out var definition))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmID}' is not registered.");
            }

            var handle = new FSMHandle(definition, context, _nextHandleID++);
            _handles.Add(handle);
            return handle;
        }

        /// <summary>Updates every valid live instance in the specified processing group.</summary>
        public void Update(int processingGroupID)
        {
            for (var i = 0; i < _handles.Count; i++)
            {
                var handle = _handles[i];
                if (handle.IsValid && handle.Definition.ProcessingGroupID == processingGroupID)
                {
                    handle.Update();
                }
            }
        }

        /// <summary>Returns the number of live handles in a processing group.</summary>
        public int GetHandleCount(int processingGroupID)
        {
            var count = 0;
            for (var i = 0; i < _handles.Count; i++)
            {
                if (_handles[i].Definition.ProcessingGroupID == processingGroupID)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
