using Api = TheSingularityWorkshop.FSM_API.FsmApi;
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
        private readonly List<PendingRegistration> _pendingRegistrations = new List<PendingRegistration>();
        private bool _isUpdating;
        private bool _isExecutingHandleUpdate;
        private bool _isFinalizingTick;
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

            // A tick is evaluated against one stable set of definitions. Registration
            // from user callbacks is committed only after every handle in that tick
            // has completed, so no handle can observe a half-old/half-new blueprint.
            if (_isUpdating)
            {
                QueueRegistration(definition, processingGroupID);
                return;
            }

            ApplyRegistration(definition, processingGroupID);
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

        /// <summary>Creates a live integer handle for a registered FSM definition. Initial state entry occurs on its first update.</summary>
        public FSMHandleInt CreateInstance(int fsmID, IStateContext context)
        {
            if (!TryGetDefinition(fsmID, out var definition))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmID}' is not registered.");
            }

            var handle = new FSMHandleInt(definition, context, _nextHandleID++);

            // Match the string-backed lifecycle: creation registers the handle but
            // does not execute user callbacks. Initial entry runs on the first update.
            // This keeps CreateInstance side-effect-free and makes both representations
            // expose the same observable lifecycle ordering.
            handle.ErrorReporter = exception => RecordInstanceError(handle, exception);
            handle.UpdateScope = ExecuteHandleUpdate;
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
            return UnregisterDefinition(fsmID, true);
        }

        private bool UnregisterDefinition(int fsmID, bool cancelPendingRegistration)
        {
            var pendingRegistrationRemoved = cancelPendingRegistration && RemovePendingRegistration(fsmID);
            if (!TryGetDefinition(fsmID, out _))
            {
                return pendingRegistrationRemoved;
            }

            _definitions[fsmID] = null;
            _processCounters[fsmID] = 0;
            _definitionErrorCounts.Remove(fsmID);
            RemoveDefinitionInstances(fsmID);

            return true;
        }

        /// <summary>Updates eligible live instances in the specified processing group.</summary>
        public void Update(int processingGroupID)
        {
            EnsureNotReentrantUpdate();
            _isUpdating = true;
            try
            {
                // Snapshot both scheduling inputs at tick start. Registrations requested
                // by callbacks remain pending, and newly created handles are not eligible
                // until the next tick.
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
            }
            finally
            {
                CompleteTick();
            }
        }

        /// <summary>Updates eligible live instances regardless of processing group.</summary>
        /// <remarks>Definition registrations requested during callbacks are committed after the complete tick.</remarks>
        public void UpdateAll()
        {
            EnsureNotReentrantUpdate();
            _isUpdating = true;
            try
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
            }
            finally
            {
                CompleteTick();
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

        /// <summary>Returns a snapshot of accumulated integer-instance error counts.</summary>
        public Dictionary<FSMHandleInt, int> GetInstanceErrorCounts()
        {
            return new Dictionary<FSMHandleInt, int>(_instanceErrorCounts);
        }

        /// <summary>Returns a snapshot of accumulated integer-definition error counts.</summary>
        public Dictionary<int, int> GetDefinitionErrorCounts()
        {
            return new Dictionary<int, int>(_definitionErrorCounts);
        }

        /// <summary>Clears the accumulated error count for one integer-backed instance.</summary>
        public void ResetInstanceErrorCount(FSMHandleInt handle)
        {
            if (handle != null)
            {
                _instanceErrorCounts.Remove(handle);
            }
        }

        /// <summary>Clears the accumulated error count for one integer-backed definition.</summary>
        public void ResetDefinitionErrorCount(int fsmID)
        {
            _definitionErrorCounts.Remove(fsmID);
        }

        private struct PendingRegistration
        {
            public readonly FSM Definition;
            public readonly int ProcessingGroupID;

            public PendingRegistration(FSM definition, int processingGroupID)
            {
                Definition = definition;
                ProcessingGroupID = processingGroupID;
            }
        }

        private void EnsureNotReentrantUpdate()
        {
            if (_isUpdating)
            {
                throw new InvalidOperationException(
                    "FSMRuntime.Update cannot be called re-entrantly from an update callback. " +
                    "The current tick must complete before another tick can begin.");
            }
        }

        private void ExecuteHandleUpdate(Action update)
        {
            if (_isUpdating)
            {
                if (_isExecutingHandleUpdate || _isFinalizingTick)
                {
                    throw new InvalidOperationException(
                        "A handle cannot be updated re-entrantly from a runtime callback or while the tick is being finalized.");
                }

                _isExecutingHandleUpdate = true;
                try
                {
                    update();
                }
                finally
                {
                    _isExecutingHandleUpdate = false;
                }
                return;
            }

            EnsureNotReentrantUpdate();
            _isUpdating = true;
            try
            {
                _isExecutingHandleUpdate = true;
                try
                {
                    update();
                }
                finally
                {
                    _isExecutingHandleUpdate = false;
                }
            }
            finally
            {
                CompleteTick();
            }
        }

        private void CompleteTick()
        {
            _isFinalizingTick = true;
            try
            {
                // Keep the runtime in its updating phase while lifecycle callbacks run.
                // Any registration they request joins the same boundary commit. Nested
                // handle updates are rejected during this phase to avoid an extra pass.
                ProcessPendingLifecycleChanges();
                ApplyPendingRegistrations();
            }
            finally
            {
                _isFinalizingTick = false;
                _isUpdating = false;
            }
        }

        private void QueueRegistration(FSM definition, int processingGroupID)
        {
            for (var i = 0; i < _pendingRegistrations.Count; i++)
            {
                if (_pendingRegistrations[i].Definition.FSM_ID == definition.FSM_ID)
                {
                    // Last registration for an identity in this tick wins. Do not
                    // mutate the active definition's group before the boundary.
                    _pendingRegistrations[i] = new PendingRegistration(definition, processingGroupID);
                    return;
                }
            }

            _pendingRegistrations.Add(new PendingRegistration(definition, processingGroupID));
        }

        private bool RemovePendingRegistration(int fsmID)
        {
            for (var i = _pendingRegistrations.Count - 1; i >= 0; i--)
            {
                if (_pendingRegistrations[i].Definition.FSM_ID == fsmID)
                {
                    _pendingRegistrations.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private void ApplyPendingRegistrations()
        {
            if (_pendingRegistrations.Count == 0)
            {
                return;
            }

            var registrations = _pendingRegistrations.ToArray();
            _pendingRegistrations.Clear();
            for (var i = 0; i < registrations.Length; i++)
            {
                ApplyRegistration(registrations[i].Definition, registrations[i].ProcessingGroupID);
            }
        }

        private void ApplyRegistration(FSM definition, int processingGroupID)
        {
            definition.ProcessingGroupID = processingGroupID;
            EnsureDefinitionCapacity(definition.FSM_ID + 1);

            if (_definitions[definition.FSM_ID] != null)
            {
                // Re-registration replaces the blueprint, not its live instances.
                // Rebinding happens atomically at the tick boundary.
                for (var i = 0; i < _handles.Count; i++)
                {
                    var handle = _handles[i];
                    if (handle.FSM_ID == definition.FSM_ID)
                    {
                        handle.Definition = definition;
                        handle.IsWaitingForValidDefinition = false;
                        if (!definition.HasState(handle.CurrentStateID))
                        {
                            handle.HasEnteredCurrentState = false;
                        }
                    }
                }

                _definitionErrorCounts.Remove(definition.FSM_ID);
            }

            _definitions[definition.FSM_ID] = definition;
            _processCounters[definition.FSM_ID] = 0;
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

                handle.Update();
            }
        }

        private void RecordInstanceError(FSMHandleInt handle, Exception exception)
        {
            var count = _instanceErrorCounts.TryGetValue(handle, out var current) ? current + 1 : 1;
            _instanceErrorCounts[handle] = count;
            Api.Error.InvokeInternalApiError(
                $"Integer FSM instance '{handle.FSM_ID}' (handle {handle.Id}, context ID: {handle.Context?.GetHashCode() ?? 0}) " +
                $"in processing group '{handle.Definition.ProcessingGroupID}' failed in state '{handle.CurrentStateID}'. " +
                $"Count: {count}/{Api.Error.InstanceErrorThreshold}.", exception);

            if (count < Api.Error.InstanceErrorThreshold || handle.IsPendingDestruction)
            {
                return;
            }

            handle.IsPendingDestruction = true;
            _pendingRemovalHandles.Add(handle);

            var definitionCount = _definitionErrorCounts.TryGetValue(handle.FSM_ID, out var previous) ? previous + 1 : 1;
            _definitionErrorCounts[handle.FSM_ID] = definitionCount;
            Api.Error.InvokeInternalApiError(
                $"Integer FSM definition '{handle.FSM_ID}' recorded a failing instance removal. " +
                $"Definition failure count: {definitionCount}/{Api.Error.DefinitionErrorThreshold}.", null);

            if (definitionCount >= Api.Error.DefinitionErrorThreshold && !_pendingUnregisterIDs.Contains(handle.FSM_ID))
            {
                _pendingUnregisterIDs.Add(handle.FSM_ID);
                Api.Error.InvokeInternalApiError(
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
                    // Automatic error-threshold teardown removes the failing instances,
                    // but a replacement explicitly requested by a callback may still be
                    // committed afterward. Explicit Unregister calls cancel earlier requests.
                    UnregisterDefinition(definitions[i], false);
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

        private void RemoveDefinitionInstances(int fsmID)
        {
            // Detach every matching handle before invoking any Exit callback. Exit
            // callbacks are user code and may re-enter this runtime; removing handles
            // one at a time while iterating _handles lets nested removals invalidate
            // the outer loop's indexes or leave siblings behind.
            var removed = new List<FSMHandleInt>();
            for (var i = _handles.Count - 1; i >= 0; i--)
            {
                var handle = _handles[i];
                if (handle.FSM_ID != fsmID)
                {
                    continue;
                }

                _handles.RemoveAt(i);
                handle.IsRegistered = false;
                handle.IsPendingDestruction = false;
                handle.ErrorReporter = null;
                _instanceErrorCounts.Remove(handle);
                removed.Add(handle);
            }

            for (var i = 0; i < removed.Count; i++)
            {
                removed[i].DestroyHandle();
            }
        }

        private void RemoveHandleAt(int index)
        {
            var handle = _handles[index];
            _handles.RemoveAt(index);
            handle.IsRegistered = false;
            handle.IsPendingDestruction = false;
            handle.ErrorReporter = null;
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
