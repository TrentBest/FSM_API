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
        /// Provides internal access to the core data structures and operational methods
        /// of the FSM API. Members within this class are not intended for direct use
        /// by external API consumers.
        /// </summary>
        public static class Internal
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
            public class FsmBucket
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
                /// <c>-1</c>: Process every frame (or every call to <see cref="Interaction.Update(string)"/>).
                /// <c>0</c>: Event-driven only (will not be processed by <see cref="Interaction.Update(string)"/>'s internal tick).
                /// <c>&gt;0</c>: Process every Nth frame, where N is the value of <c>ProcessRate</c>.
                /// </value>
                public int ProcessRate;

                /// <summary>
                /// An internal counter used for frame-skipping when <see cref="ProcessRate"/> is greater than 0.
                /// </summary>
                public int Counter;
            }

            /// <summary>
            /// A queue of actions representing modifications to the FSM system
            /// that are deferred to prevent collection modification errors during iteration.
            /// </summary>
            /// <remarks>
            /// Operations like adding or removing FSM instances/definitions are
            /// enqueued and processed at safe points in the update cycle (e.g., after all FSMs have ticked).
            /// </remarks>
            private static readonly Queue<Action> _deferredModifications = new();

            /// <summary>
            /// Stores all FSM definitions and their instances, organized by
            /// their processing group (e.g., "Update", "FixedUpdate") and then by FSM name.
            /// </summary>
            /// <remarks>
            /// This nested dictionary forms the central registry for the FSM system,
            /// allowing efficient lookup and management of FSM blueprints and their live
            /// manifestations. Access to this collection should be strictly confined to the single
            /// designated thread where FSM updates occur.
            /// </remarks>
            private static Dictionary<string, Dictionary<string, FsmBucket>> _buckets = new();

            /// <summary>
            /// Represents a default FSM definition for internal API operations.
            /// </summary>
            private static FSM _defaultFSM = new FSM();


            /// <summary>
            /// Sets a time limit (in milliseconds) for how long an FSM processing group update can take
            /// before a warning message is logged via OnInternalApiError. Helps identify performance bottlenecks.
            /// Default is 5ms.
            /// </summary>
            public static long TickPerformanceWarningThresholdMs { get; set; } = 5;


            /// <summary>
            /// Gets the total number of distinct processing groups (e.g., "Update", "FixedUpdate").
            /// This is correct as you had it.
            /// </summary>
            public static int ProcessingGroupCount => _buckets.Count;

            /// <summary>
            /// Gets the total number of unique FSM definitions across all processing groups.
            /// Your original `FiniteStateMachineCount` was correct for this.
            /// </summary>
            public static int TotalFsmDefinitionCount => _buckets.Sum(group => group.Value.Count);

            /// <summary>
            /// Gets the total number of active FSM instances (handles) across all FSM definitions
            /// within all processing groups.
            /// This requires summing the 'Instances' list from each FsmBucket.
            /// </summary>
            public static int TotalFsmHandleCount => _buckets.Sum(
                group => group.Value.Sum(fsm => fsm.Value.Instances.Count)
            );

            /// <summary>
            /// Gets the number of FSM definitions within a specific processing group.
            /// </summary>
            /// <param name="processingGroupName">The name of the processing group.</param>
            /// <returns>The count of FSM definitions, or 0 if the group doesn't exist.</returns>
            public static int GetFsmDefinitionCountInGroup(string processingGroupName)
            {
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.Count;
                }
                return 0;
            }

            /// <summary>
            /// Gets the total number of active FSM instances (handles) within a specific processing group.
            /// </summary>
            /// <param name="processingGroupName">The name of the processing group.</param>
            /// <returns>The count of handles, or 0 if the group doesn't exist.</returns>
            public static int GetFsmHandleCountInGroup(string processingGroupName)
            {
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.Sum(fsm => fsm.Value.Instances.Count);
                }
                return 0;
            }

            /// <summary>
            /// Gets the average number of FSM definitions per processing group.
            /// </summary>
            public static double AverageFsmDefinitionsPerGroup
            {
                get
                {
                    if (_buckets.Count == 0) return 0;
                    return (double)TotalFsmDefinitionCount / _buckets.Count;
                }
            }

            /// <summary>
            /// Gets the average number of FSM handles per FSM definition (across all definitions).
            /// </summary>
            public static double AverageFsmHandlesPerDefinition
            {
                get
                {
                    int totalDefinitions = TotalFsmDefinitionCount;
                    if (totalDefinitions == 0) return 0;
                    return (double)TotalFsmHandleCount / totalDefinitions;
                }
            }

            /// <summary>
            /// Gets the names of all currently registered processing groups.
            /// </summary>
            public static IEnumerable<string> GetProcessingGroupNames()
            {
                return _buckets.Keys;
            }

            /// <summary>
            /// Gets the names of all FSM definitions within a specific processing group.
            /// </summary>
            /// <param name="processingGroupName">The name of the processing group.</param>
            /// <returns>An enumerable of FSM names, or empty if the group doesn't exist.</returns>
            public static IEnumerable<string> GetFsmDefinitionNamesInGroup(string processingGroupName)
            {
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.Keys;
                }
                return Enumerable.Empty<string>();
            }

            /// <summary>
            /// Checks if a specific FSM definition exists within a given processing group.
            /// </summary>
            public static bool DoesFsmDefinitionExist(string processingGroupName, string fsmName)
            {
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.ContainsKey(fsmName);
                }
                return false;
            }

            /// <summary>
            /// Gets a flattened list of all active FSM Handles across all processing groups and FSM definitions.
            /// </summary>
            public static IEnumerable<FSMHandle> GetAllFsmHandles()
            {
                return _buckets.SelectMany(group =>
                    group.Value.SelectMany(fsm => fsm.Value.Instances)
                );
            }

            /// <summary>
            /// Internally retrieves the dictionary of <see cref="FsmBucket"/>s for a given processing group.
            /// If the group does not exist, it will be created automatically.
            /// </summary>
            /// <remarks>
            /// This method is designed for **internal use** within the <see cref="FSM_API"/> assembly
            /// and is not part of the public API surface.
            /// </remarks>
            /// <param name="processingGroup">The name of the processing group (e.g., "Update", "FixedUpdate").</param>
            /// <returns>A dictionary containing FSM buckets for the specified processing group, keyed by FSM name.</returns>
            /// <exception cref="ArgumentException">Thrown if <paramref name="processingGroup"/> is null or empty.</exception>
            internal static Dictionary<string, FsmBucket> GetOrCreateBucketProcessingGroup(string processingGroup)
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
            /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.</exception>
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
                    Error.InvokeInternalApiError($"FSM '{fsmName}' in processing group '{processingGroup}' definition updated at runtime.", new Exception());
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
                    Error.InvokeInternalApiError($"FSM '{fsmName}' in processing group '{processingGroup}' newly registered.", new Exception());
                }
            }

            /// <summary>
            /// Executes any pending modifications (e.g., adding/removing FSM instances)
            /// that were deferred during the FSM update cycle. This ensures collection safety.
            /// </summary>
            /// <remarks>
            /// This method is called internally by <see cref="Interaction.Update(string)"/>
            /// and should not typically be called directly by users. It guarantees that
            /// structural changes to the FSM system (like unregistering instances that
            /// hit an error threshold) happen safely after all FSMs for a given tick
            /// have completed their processing.
            /// Errors during deferred actions are reported via <see cref="Error.OnInternalApiError"/>.
            /// </remarks>
            internal static void ProcessDeferredModifications()
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
                        Error.InvokeInternalApiError("Error during deferred API modification.", ex);
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
            /// reported via <see cref="Error.ReportError(FSMHandle, Exception)"/>, leading
            /// to automatic removal if error thresholds are exceeded.
            /// </remarks>
            internal static void TickAll(string processingGroup)
            {
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    Error.InvokeInternalApiError("TickAll called with null or empty processing group.", null);
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
                    foreach (var handle in instancesToTick)
                    {
                        // Ensure the handle itself is not null, its context is not null,
                        // AND its context is reported as valid by the context itself.
                        if (handle != null && handle.Context != null && handle.Context.IsValid)
                        {
                            try
                            {
                                handle.Update();
                            }
                            catch (Exception ex)
                            {
                                // If an exception occurs during the FSM's internal step, report it as a "error".
                                Error.InvokeInstanceError(handle,
                                    $"",
                                    ex, processingGroup);
                            }
                        }
                        else
                        {
                            // If an instance is null or its context is null/invalid, it should be removed.
                            Error.InvokeInstanceError(handle==null?null:handle, "FSM instance or its context became null/invalid (IsValid returned false).", new ApplicationException("FSM instance or its context became null/invalid (IsValid returned false)."), processingGroup);
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the queue of deferred modifications.
            /// </summary>
            internal static Queue<Action> GetDeferred()
            {
                return _deferredModifications;
            }

            /// <summary>
            /// Gets the internal default FSM definition.
            /// </summary>
            internal static FSM GetDefaultFSM()
            {
                return _defaultFSM;
            }

            /// <summary>
            /// Gets the internal collection of FSM processing buckets.
            /// </summary>
            /// <remarks>
            /// This accessor exposes the central internal data structure of the FSM system.
            /// It is intended for advanced internal diagnostics or specific integration scenarios
            /// and should be used with caution, respecting the single-threaded update model.
            /// </remarks>
            public static Dictionary<string, Dictionary<string, FsmBucket>> GetBuckets()
            {
                return _buckets;
            }

            /// <summary>
            /// Retrieves a specific Finite State Machine (FSM) definition by its name within a given processing group.
            /// </summary>
            /// <remarks>
            /// This is an **internal-use only** method, primarily utilized by the FSM_API
            /// for managing and accessing registered FSM blueprints. It provides a safe mechanism
            /// to attempt retrieval of an FSM definition without raising exceptions if the specified
            /// processing group or the FSM itself does not currently exist.
            /// </remarks>
            /// <param name="name">The unique name of the FSM definition to retrieve.</param>
            /// <param name="processingGroup">The name of the processing group where the FSM definition is expected to be registered.
            /// Defaults to "Update" if not specified.</param>
            /// <returns>
            /// The <see cref="FSM"/> definition object if found; otherwise, <c>null</c> if either the specified
            /// <paramref name="processingGroup"/> does not exist or an FSM with the given <paramref name="name"/>
            /// is not registered within that group.
            /// </returns>
            internal static FSM? GetFSM(string name, string processingGroup = "Update")
            {
                // Step 1: Safely attempt to get the inner dictionary for the processing group.
                // Using TryGetValue prevents a KeyNotFoundException if the processingGroup doesn't exist.
                if (!_buckets.TryGetValue(processingGroup, out var groupMap))
                {
                    // The specified processing group does not exist.
                    // For internal methods, you might log this as a debug warning, but for a 'Get' method,
                    // returning null is the expected graceful failure.
                    return null;
                }

                // Step 2: Safely attempt to get the FsmBucket for the specific FSM name within that group.
                // Using TryGetValue prevents a KeyNotFoundException if the FSM name doesn't exist in the group.
                if (groupMap.TryGetValue(name, out var fsmBucket))
                {
                    // Both the group and the FSM were found. Return its Definition.
                    return fsmBucket.Definition == null ? GetDefaultFSM() : fsmBucket.Definition;
                }
                else
                {
                    // The FSM with the given name was not found within the specified processing group.
                    return GetDefaultFSM();
                }
            }

            /// <summary>
            /// Retrieves the FSM bucket for a specific FSM definition and processing group.
            /// </summary>
            /// <param name="fsmName">The name of the FSM definition.</param>
            /// <param name="processGroup">The name of the processing group.</param>
            /// <returns>The <see cref="FsmBucket"/> if found; otherwise, <c>null</c>.</returns>
            /// <remarks>
            /// This method provides direct access to the internal FSM bucket structure.
            /// It includes safety checks to prevent <see cref="NullReferenceException"/>
            /// and <see cref="KeyNotFoundException"/> if the specified group or FSM name does not exist.
            /// </remarks>
            internal static FsmBucket GetBucket(string fsmName, string processGroup)
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    Error.InvokeInternalApiError("GetBucket: FSM name cannot be null or empty.", new ArgumentException(nameof(fsmName)));
                    return null;
                }
                if (string.IsNullOrWhiteSpace(processGroup))
                {
                    Error.InvokeInternalApiError("GetBucket: Process group cannot be null or empty.", new ArgumentException(nameof(processGroup)));
                    return null;
                }

                // Try to get the dictionary for the specific processing group
                if (_buckets.TryGetValue(processGroup, out Dictionary<string, FsmBucket> groupBuckets))
                {
                    // Try to get the specific FsmBucket within that group
                    if (groupBuckets.TryGetValue(fsmName, out FsmBucket fsmBucket))
                    {
                        return fsmBucket;
                    }
                    else
                    {
                        // Optional: Log if the FSM name isn't found in the group
                        // Error.InvokeInternalApiError($"GetBucket: FSM definition '{fsmName}' not found in processing group '{processGroup}'.", null);
                        return null;
                    }
                }
                else
                {
                    // Optional: Log if the processing group isn't found
                    // Error.InvokeInternalApiError($"GetBucket: Processing group '{processGroup}' not found.", null);
                    return null;
                }
            }

            public static void ResetAPI(bool hardReset = false)
            {
                if (hardReset)
                {
                    _buckets.Clear();


                }
                else
                {
                    var bucks = _buckets.ToList();
                    foreach (KeyValuePair<string, Dictionary<string, FsmBucket>> processGroup in bucks)
                    {
                        var buckets = processGroup.Value.Values.ToList();
                        foreach (FsmBucket bucket in buckets)
                        {
                            var handles = bucket.Instances.ToList();
                            foreach (var handle in handles)
                            {
                                handle.TransitionTo(handle.Definition.InitialState);
                                FSM_API.Interaction.Unregister(handle);
                            }

                        }
                    }
                    _buckets.Clear();

                }
                _deferredModifications.Clear();
                Error.Reset();
            }


            //Dictionary<string, Dictionary<string, FsmBucket>> 


            internal static void SetProcessGroup(string processGroup, FsmBucket bucket)
            {
                if (!_buckets.ContainsKey(processGroup))
                {
                    Create.CreateProcessingGroup(processGroup);
                }
                //remove the bucket from it's current process group
                _buckets.First(pg => pg.Value.ContainsKey(bucket.Definition.Name)).Value.Remove(bucket.Definition.Name);
                _buckets[processGroup].Add(bucket.Definition.Name, bucket);
            }

            public static List<string> GetProcessingGroups()
            {
                List<string> groups = new List<string>();
                foreach (var processingGroup in _buckets)
                {
                    groups.Add(processingGroup.Key);
                }
                return groups;
            }
        }
    }
}
