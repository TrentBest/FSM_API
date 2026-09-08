using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Owns integer-backed FSM definitions and their live handles.
    /// </summary>
    /// <remarks>
    /// FSM identities are indexes into the definition array. The runtime therefore resolves an FSM
    /// definition by direct integer indexing rather than hashing an integer key. State identities use
    /// the same model inside <see cref="FSM"/>. The string-backed API remains a separate representation
    /// and is translated at the boundary rather than consulted by this runtime.
    /// </remarks>
    public sealed class FSMRuntime
    {
        private FSM[] _definitions = new FSM[0];
        private int[] _processCounters = new int[0];
        private readonly List<FSMHandleInt> _handles = new List<FSMHandleInt>();
        private int _nextHandleID;

        /// <summary>Registers an integer-backed FSM definition for a processing group.</summary>
        public void Register(FSM definition, int processingGroupID = 0)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.FSM_ID < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(definition), "FSM IDs must be non-negative array indexes.");
            }

            definition.ProcessingGroupID = processingGroupID;
            EnsureDefinitionCapacity(definition.FSM_ID + 1);

            // A replacement definition supersedes the previous definition and its live instances.
            if (_definitions[definition.FSM_ID] != null)
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
            _processCounters[definition.FSM_ID] = 0;
        }

        /// <summary>Returns whether an FSM definition is registered.</summary>
        public bool Contains(int fsmID)
        {
            return TryGetDefinition(fsmID, out _);
        }

        /// <summary>Gets a registered FSM definition by integer identity.</summary>
        public FSM GetDefinition(int fsmID)
        {
            return TryGetDefinition(fsmID, out var definition) ? definition : null;
        }

        /// <summary>Creates and initializes a live integer handle for a registered FSM definition.</summary>
        /// <remarks>
        /// Runtime creation owns the instance lifecycle boundary: construction creates the explicitly
        /// integer-backed handle, then initialization enters the initial state exactly once before the
        /// handle is returned.
        /// </remarks>
        public FSMHandleInt CreateInstance(int fsmID, IStateContext context)
        {
            if (!TryGetDefinition(fsmID, out var definition))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmID}' is not registered.");
            }

            var handle = new FSMHandleInt(definition, context, _nextHandleID++);
            handle.Initialize();
            _handles.Add(handle);
            return handle;
        }

        /// <summary>Removes a live integer handle from this runtime.</summary>
        /// <returns><c>true</c> when the handle was registered with this runtime.</returns>
        public bool RemoveInstance(FSMHandleInt handle)
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
            if (!TryGetDefinition(fsmID, out _))
            {
                return false;
            }

            _definitions[fsmID] = null;
            _processCounters[fsmID] = 0;

            for (var i = _handles.Count - 1; i >= 0; i--)
            {
                if (_handles[i].FSM_ID == fsmID)
                {
                    _handles.RemoveAt(i);
                }
            }

            return true;
        }

        /// <summary>Updates eligible live instances in the specified processing group.</summary>
        public void Update(int processingGroupID)
        {
            for (var i = 0; i < _definitions.Length; i++)
            {
                var definition = _definitions[i];
                if (definition == null || definition.ProcessingGroupID != processingGroupID || !ShouldProcess(definition))
                {
                    continue;
                }

                UpdateDefinitionInstances(definition.FSM_ID);
            }
        }

        /// <summary>Updates eligible live instances regardless of processing group.</summary>
        public void UpdateAll()
        {
            for (var i = 0; i < _definitions.Length; i++)
            {
                var definition = _definitions[i];
                if (definition != null && ShouldProcess(definition))
                {
                    UpdateDefinitionInstances(definition.FSM_ID);
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

        private bool ShouldProcess(FSM definition)
        {
            if (definition.ProcessRate < 0)
            {
                return true;
            }

            if (definition.ProcessRate == 0)
            {
                return false;
            }

            var counter = _processCounters[definition.FSM_ID] + 1;
            if (counter < definition.ProcessRate)
            {
                _processCounters[definition.FSM_ID] = counter;
                return false;
            }

            _processCounters[definition.FSM_ID] = 0;
            return true;
        }

        private void UpdateDefinitionInstances(int fsmID)
        {
            for (var i = 0; i < _handles.Count; i++)
            {
                var handle = _handles[i];
                if (handle.FSM_ID == fsmID && handle.IsValid)
                {
                    handle.Update();
                }
            }
        }

        private bool TryGetDefinition(int fsmID, out FSM definition)
        {
            if (fsmID >= 0 && fsmID < _definitions.Length)
            {
                definition = _definitions[fsmID];
                return definition != null;
            }

            definition = null;
            return false;
        }

        private void EnsureDefinitionCapacity(int requiredLength)
        {
            if (requiredLength <= _definitions.Length)
            {
                return;
            }

            var newLength = _definitions.Length == 0 ? 4 : _definitions.Length;
            while (newLength < requiredLength)
            {
                newLength *= 2;
            }

            Array.Resize(ref _definitions, newLength);
            Array.Resize(ref _processCounters, newLength);
        }
    }
}
