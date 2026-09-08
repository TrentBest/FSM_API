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

            // A replacement definition supersedes the previous definition and its live instances.
            // Do not leave handles attached to a definition that is no longer registered.
            if (_definitions.ContainsKey(definition.FSM_ID))
            {
                for (var i = _handles.Count - 1; i >= 0; i--)
                {
                    if (_handles[i].FSM_ID == definition.FSM_ID)
                    {
                        _handles.RemoveAt(i);
                    }
                }
            }

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

        /// <summary>Creates and initializes a live handle for a registered FSM definition.</summary>
        /// <remarks>
        /// Runtime creation owns the instance lifecycle boundary: construction creates the handle,
        /// then initialization enters the initial state exactly once before the handle is returned.
        /// </remarks>
        public FSMHandle CreateInstance(int fsmID, IStateContext context)
        {
            if (!_definitions.TryGetValue(fsmID, out var definition))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmID}' is not registered.");
            }

            var handle = new FSMHandle(definition, context, _nextHandleID++);
            handle.Initialize();
            _handles.Add(handle);
            return handle;
        }

        /// <summary>Removes a live handle from this runtime.</summary>
        /// <returns><c>true</c> when the handle was registered with this runtime.</returns>
        public bool RemoveInstance(FSMHandle handle)
        {
            if (handle == null)
            {
                return false;
            }

            return _handles.Remove(handle);
        }

        /// <summary>
        /// Unregisters an FSM definition and removes its live instances from this runtime.
        /// </summary>
        /// <returns><c>true</c> when a definition was registered under the supplied ID.</returns>
        public bool Unregister(int fsmID)
        {
            if (!_definitions.Remove(fsmID))
            {
                return false;
            }

            for (var i = _handles.Count - 1; i >= 0; i--)
            {
                if (_handles[i].FSM_ID == fsmID)
                {
                    _handles.RemoveAt(i);
                }
            }

            return true;
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

        /// <summary>Updates every valid live instance regardless of processing group.</summary>
        /// <remarks>
        /// This provides a single runtime tick for hosts that do not need group-specific scheduling.
        /// Group-specific callers should continue to use <see cref="Update(int)"/>.
        /// </remarks>
        public void UpdateAll()
        {
            for (var i = 0; i < _handles.Count; i++)
            {
                var handle = _handles[i];
                if (handle.IsValid)
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
