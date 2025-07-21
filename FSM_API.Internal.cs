using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// This partial class contains the internal data structures and helper methods
    /// essential for the operation of the <see cref="FSM_API"/>.
    /// </summary>
    /// <remarks>
    /// While the <see cref="FSM_API"/> class itself is public, the members within this
    /// partial file are generally marked <c>private</c> or <c>internal</c>. This
    /// ensures that the core mechanics and state management of the FSM system are
    /// encapsulated and not directly exposed to external API consumers, maintaining
    /// a clean and controlled public interface.
    /// <para>
    /// **Important:** The declaration `public static partial class FSM_API` must
    /// be consistent across all partial files defining the <see cref="FSM_API"/> class.
    /// </para>
    /// </remarks>
    public static partial class FSM_API
    {
        /// <summary>
        /// Represents an internal container for an FSM definition and all its active instances.
        /// </summary>
        /// <remarks>
        /// This class is used **internally** by the <see cref="FSM_API"/> to efficiently manage
        /// FSM definitions and their associated running instances within specific
        /// processing groups. It tracks the FSM's blueprint, its live instances,
        /// and properties related to its update rate. Being <c>private</c>, it's
        /// completely encapsulated within the <see cref="FSM_API"/> class.
        /// </remarks>
        private class FsmBucket
        {
            /// <summary>
            /// The blueprint (definition) of the Finite State Machine.
            /// </summary>
            public FSM? Definition;

            /// <summary>
            /// A list of all active <see cref="FSMHandle"/> instances created from this definition.
            /// </summary>
            /// <remarks>
            /// Instances are added to this list upon creation and removed upon unregistration
            /// or automatic removal due to error thresholds.
            /// </remarks>
            public List<FSMHandle> Instances = new();

            /// <summary>
            /// The desired processing rate for instances of this FSM definition.
            /// </summary>
            /// <value>
            /// <c>-1</c>: Process every frame (or every call to <see cref="Update(string)"/>).
            /// <c>0</c>: Event-driven only (will not be processed by <see cref="Update(string)"/>'s internal tick).
            /// <c>&gt;0</c>: Process every Nth frame, where N is the value of <c>ProcessRate</c>.
            /// </value>
            public int ProcessRate;

            /// <summary>
            /// An internal counter used for frame-skipping when <see cref="ProcessRate"/> is greater than 0.
            /// </summary>
            public int Counter;          // Internal counter for frame-skipping

        }

        /// <summary>
        /// A queue of actions representing modifications to the FSM system
        /// that are deferred to prevent collection modification errors during iteration.
        /// </summary>
        /// <remarks>
        /// Operations like adding or removing FSM instances/definitions are
        /// enqueued and processed at safe points in the update cycle (e.g., after all FSMs have ticked).
        /// Consider using <see cref="System.Collections.Concurrent.ConcurrentQueue{T}"/> for thread safety
        /// if enqueue operations can happen from different threads.
        /// </remarks>
        private static readonly Queue<Action> _deferredModifications = new();

        





        /// <summary>
        /// Stores all FSM definitions and their instances, organized by
        /// their processing group (e.g., "Update", "FixedUpdate") and then by FSM name.
        /// </summary>
        /// <remarks>
        /// This nested dictionary forms the central registry for the FSM system,
        /// allowing efficient lookup and management of FSM blueprints and their live
        /// manifestations. Access to this collection should be strictly confined to a single
        /// thread or protected by locks if concurrent access is needed.
        /// </remarks>
        private static Dictionary<string, Dictionary<string, FsmBucket>> _buckets = new();

        /// <summary>
        /// Represents a default FSM definition.
        /// </summary>
        /// <remarks>
        /// Currently initialized as a new, unconfigured <see cref="FSM"/> object.
        /// Consider if this should be a fully defined default FSM registered via
        /// <see cref="Register(string, FSM, int, string)"/> during an initialization phase,
        /// or if its purpose is solely to prevent null references.
        /// A robust default FSM would typically have a name, processing group, and initial state.
        /// </remarks>
        private static FSM _defaultFSM = new FSM();//<= not sure about this... as in it isn't a complete thought...




        /// <summary>
        /// Safely retrieves the dictionary of <see cref="FsmBucket"/>s for a given processing group.
        /// If the group does not exist, it will be created automatically.
        /// </summary>
        /// <remarks>
        /// This method is designed for **internal use** within the <see cref="FSM_API"/> assembly.
        /// </remarks>
        /// <param name="processingGroup">The name of the processing group (e.g., "Update", "FixedUpdate").</param>
        /// <returns>A dictionary containing FSM buckets for the specified processing group, keyed by FSM name.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="processingGroup"/> is null or empty.</exception>

        private static Dictionary<string, FsmBucket> GetOrCreateBucketProcessingGroup(string processingGroup)
        {
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets))
            {
                categoryBuckets = new Dictionary<string, FsmBucket>();
                _buckets[processingGroup] = categoryBuckets;
            }
            return categoryBuckets;
        }










        /// <summary>
        /// Internally registers or updates a fully defined FSM into the system. This method is called by
        /// <see cref="FSMBuilder.BuildDefinition()"/> and should not be invoked directly by users.
        /// </summary>
        /// <param name="fsmName">The unique name of the FSM definition.</param>
        /// <param name="fsm">The complete <see cref="FSM"/> definition object.</param>
        /// <param name="processRate">The update rate for this FSM.</param>
        /// <param name="processingGroup">The processing group this FSM belongs to.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/>, <paramref name="fsm"/>, or <paramref name="processingGroup"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fsm"/> is null.</exception>
        internal static void Register(
            string fsmName,
            FSM fsm,
            int processRate, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty during registration.", nameof(fsmName));
            }
            if (fsm == null)
            {
                throw new ArgumentNullException(nameof(fsm), "FSM definition cannot be null during registration.");
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty during registration.", nameof(processingGroup));
            }

            var buckets = GetOrCreateBucketProcessingGroup(processingGroup);
            if (buckets.TryGetValue(fsmName, out var existingBucket))
            {
                // FSM already exists, update its definition and process rate.
                // Existing instances will automatically use the new definition on their next tick.
                existingBucket.Definition = fsm;
                existingBucket.ProcessRate = processRate;
                existingBucket.Counter = processRate > 0 ? processRate : 0; // Reset counter for new rate
                InvokeInternalApiError($"FSM '{fsmName}' in processing group '{processingGroup}' definition updated at runtime.", null);
            }
            else
            {
                // New FSM definition
                buckets[fsmName] = new FsmBucket
                {
                    Definition = fsm,
                    ProcessRate = processRate,
                    Counter = processRate > 0 ? processRate : 0,
                };
                InvokeInternalApiError($"FSM '{fsmName}' in processing group '{processingGroup}' newly registered.", null);
            }
        }





        /// <summary>
        /// Executes any pending modifications (e.g., adding/removing FSM instances)
        /// that were deferred during the FSM update cycle. This ensures collection safety.
        /// </summary>
        /// <remarks>
        /// This method is called internally by <see cref="Update(string)"/>
        /// and should not typically be called directly by users. It guarantees that
        /// structural changes to the FSM system (like unregistering instances that
        /// hit an error threshold) happen safely after all FSMs for a given tick
        /// have completed their processing.
        /// Errors during deferred actions are reported via <see cref="OnInternalApiError"/>.
        /// </remarks>
        private static void ProcessDeferredModifications()
        {
            while (_deferredModifications.Count > 0)
            {
                var action = _deferredModifications.Dequeue();
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    InvokeInternalApiError("Error during deferred API modification.", ex);
                }
            }
        }





        /// <summary>
        /// The internal core method that ticks (updates) all FSM instances within a
        /// specified processing group, respecting each FSM's defined process rate.
        /// </summary>
        /// <param name="processingGroup">The group of FSMs to tick (e.g., "Update", "FixedUpdate").</param>
        /// <remarks>
        /// This method iterates through all registered FSM definitions in the given
        /// <paramref name="processingGroup"/>. For each definition, it checks its
        /// <see cref="FsmBucket.ProcessRate"/> and only ticks the instances if
        /// the rate criteria are met. Instances with a <c>ProcessRate</c> of <c>0</c>
        /// (event-driven) are explicitly skipped by this tick mechanism.
        /// Individual FSM instance updates that throw exceptions are caught and
        /// reported via <see cref="ReportError(FSMHandle, Exception)"/>, leading
        /// to automatic removal if error thresholds are exceeded.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Although handled internally with `InvokeInternalApiError`, conventionally
        /// this would throw if `processingGroup` is null or empty, but the internal
        /// error reporting system is used instead for resilience.
        /// </exception>
        private static void TickAll(string processingGroup)
        {
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                InvokeInternalApiError("TickAll called with null or empty processing group.", null);
                return;
            }

            if (!_buckets.TryGetValue(processingGroup, out var fsmDefinitionsForCategory))
            {
                return;
            }

            var bucketsToTick = fsmDefinitionsForCategory.Values.ToList();

            foreach (var bucket in bucketsToTick)
            {
                if (bucket.ProcessRate == 0)
                {
                    continue;
                }

                if (bucket.ProcessRate > 0)
                {
                    bucket.Counter--;
                    if (bucket.Counter > 0)
                    {
                        continue;
                    }
                    bucket.Counter = bucket.ProcessRate;
                }

                var instancesToTick = bucket.Instances.ToList();
                foreach (var h in instancesToTick)
                {
                    // Ensure the handle itself is not null, its context is not null,
                    // AND its context is reported as valid by the context itself.
                    if (h != null && h.Context != null && h.Context.IsValid)
                    {
                        try
                        {
                            h.Update();
                        }
                        catch (Exception ex)
                        {
                            // If an exception occurs during the FSM's internal step, report it as a "error".
                            ReportError(h, ex);
                        }
                    }
                    else
                    {
                        // If an instance is null or its context is null/invalid, it should be removed.
                        ReportError(h, new ApplicationException("FSM instance or its context became null/invalid (IsValid returned false)."));
                    }
                }
            }
        }
    }
}