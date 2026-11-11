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
        private readonly Dictionary<FSMHandleInt, int> _instanceErrorCounts = new Dictionary<FSMHandleInt, int>();
        private readonly Dictionary<int, int> _definitionErrorCounts = new Dictionary<int, int>();
        private readonly List<FSMHandleInt> _pendingRemovalHandles = new List<FSMHandleInt>();
        private readonly List<int> _pendingUnregisterIDs = new List<int>();
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

            if (_definitions[definition.FSM_ID] != null)
            {
                _definitionErrorCounts.Remove(definition.FSM_ID);
                for (var i = _handles.Count - 1; i >= 0; i--)
                {
                    if (_handles[i].FSM_ID == definition.FSM_ID)
                    {
                        RemoveHandleAt(i);
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
        public FSMHandleInt CreateInstance(int fsmID, IStateContext context)
        {
            if (!TryGetDefinition(fsmID, out var definition))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmID}' is not registered.");
            }

            var handle = new FSMHandleInt(definition, context, _nextHandleID++);
            handle.Initialize();
            handle.IsRegistered = true;
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

            var index = _handles.IndexOf(handle);
            if (index < 0)
            {
                return false;
            }

            RemoveHandleAt(index);
            return true;
        }

        /// <summary>
        /// Returns the live handles belonging to an integer-backed FSM definition.
        /// </summary>
        /// <remarks>
        /// This is an inspection/modification path, not the runtime update hot path. The returned
        /// collection is a snapshot so callers can safely modify instances without invalidating the
        /// runtime's internal handle list.
        /// </remarks>
        public IReadOnlyCollection<FSMHandleInt> GetInstances(int fsmID)
        {
            var instances = new List<FSMHandleInt>();
            for (var i = 0; i < _handles.Count; i++)
            {
                if (_handles[i].FSM_ID == fsmID)
                {
                    instances.Add(_handles[i]);
                }
            }

            return instances.AsReadOnly();
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
            _definitionErrorCounts.Remove(fsmID);

            for (var i = _handles.Count - 1; i >= 0; i--)
            {
                if (_handles[i].FSM_ID == fsmID)
                {
                    RemoveHandleAt(i);
                }
            }

            return true;
        }

        /// <summary>Updates eligible live instances in the specified processing group.</summary>
        public void Update(int processingGroupID)
        {
            // Snapshot the handles at the start of the tick. Handles created by callbacks are
            // registered immediately but are not eligible to update until the next tick.
            var handlesAtTickStart = _handles.ToArray();
            var definitionCountAtTickStart = _definitions.Length;

            for (var i = 0; i < definitionCountAtTickStart; i++)
            {
                var definition = _definitions[i];
                if (definition == null || definition.ProcessingGroupID != processingGroupID || !ShouldProcess(definition))
                {
                    continue;
                }

                UpdateDefinitionInstances(definition.FSM_ID, handlesAtTickStart);
            }

            ProcessPendingLifecycleChanges();
        }

        /// <summary>Updates eligible live instances regardless of processing group.</summary>
        public void UpdateAll()
        {
            var handlesAtTickStart = _handles.ToArray();
            var definitionCountAtTickStart = _definitions.Length;

            for (var i = 0; i < definitionCountAtTickStart; i++)
            {
                var definition = _definitions[i];
                if (definition != null && ShouldProcess(definition))
                {
                    UpdateDefinitionInstances(definition.FSM_ID, handlesAtTickStart);
                }
            }

            ProcessPendingLifecycleChanges();
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

        private void UpdateDefinitionInstances(int fsmID, FSMHandleInt[] handlesAtTickStart)
        {
            for (var i = 0; i < handlesAtTickStart.Length; i++)
            {
                var handle = handlesAtTickStart[i];
                if (!handle.IsRegistered || handle.IsDestroyed || handle.IsPendingDestruction ||
                    handle.FSM_ID != fsmID || !handle.IsValid)
                {
                    continue;
                }

                try
                {
                    handle.Update();
                }
                catch (Exception ex)
                {
                    RecordInstanceError(handle, ex);
                }
            }
        }

        private void RecordInstanceError(FSMHandleInt handle, Exception exception)
        {
            var count = _instanceErrorCounts.TryGetValue(handle, out var current) ? current + 1 : 1;
            _instanceErrorCounts[handle] = count;
            FSM_API.Error.InvokeInternalApiError(
                $"Integer FSM instance '{handle.FSM_ID}' (handle {handle.Id}, context ID: {handle.Context?.GetHashCode() ?? 0}) " +
                $"in processing group '{handle.Definition.ProcessingGroupID}' failed in state '{handle.CurrentStateID}'. " +
                $"Count: {count}/{FSM_API.Error.InstanceErrorThreshold}.", exception);

            if (count < FSM_API.Error.InstanceErrorThreshold || handle.IsPendingDestruction)
            {
                return;
            }

            handle.IsPendingDestruction = true;
            _pendingRemovalHandles.Add(handle);

            var definitionCount = _definitionErrorCounts.TryGetValue(handle.FSM_ID, out var previous) ? previous + 1 : 1;
            _definitionErrorCounts[handle.FSM_ID] = definitionCount;
            FSM_API.Error.InvokeInternalApiError(
                $"Integer FSM definition '{handle.FSM_ID}' recorded a failing instance removal. " +
                $"Definition failure count: {definitionCount}/{FSM_API.Error.DefinitionErrorThreshold}.", null);

            if (definitionCount >= FSM_API.Error.DefinitionErrorThreshold && !_pendingUnregisterIDs.Contains(handle.FSM_ID))
            {
                _pendingUnregisterIDs.Add(handle.FSM_ID);
                FSM_API.Error.InvokeInternalApiError(
                    $"Integer FSM definition '{handle.FSM_ID}' reached its definition error threshold. " +
                    "Unregistration is scheduled for the end of this update tick.", null);
            }
        }

        private void ProcessPendingLifecycleChanges()
        {
            if (_pendingUnregisterIDs.Count > 0)
            {
                var definitions = _pendingUnregisterIDs.ToArray();
                _pendingUnregisterIDs.Clear();
                for (var i = 0; i < definitions.Length; i++)
                {
                    Unregister(definitions[i]);
                }
            }

            if (_pendingRemovalHandles.Count > 0)
            {
                var handles = _pendingRemovalHandles.ToArray();
                _pendingRemovalHandles.Clear();
                for (var i = 0; i < handles.Length; i++)
                {
                    var handle = handles[i];
                    if (handle.IsRegistered)
                    {
                        RemoveInstance(handle);
                    }
                }
            }
        }

        private void RemoveHandleAt(int index)
        {
            var handle = _handles[index];
            _handles.RemoveAt(index);
            handle.IsRegistered = false;
            handle.IsPendingDestruction = false;
            _instanceErrorCounts.Remove(handle);
            handle.DestroyHandle();
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
