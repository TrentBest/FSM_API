

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
    /// **Important:** The declaration <c>public static partial class FSM_API</c> must
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
            /// and properties related to its update rate. Being <c>public</c> (nested within an internal class),
            /// it's effectively encapsulated within the <see cref="FSM_API"/>'s internal structure.
            /// </remarks>
            public class FsmBucket
            {
                /// <summary>
                /// The blueprint (definition) of the Finite State Machine.
                /// </summary>
                // Nullability: FSM can be null if it's not yet assigned, but it should be set before use.
                // If it can genuinely be null (e.g., during initialization before being fully defined),
                // FSM? is correct. Given where it's used, it should ideally be non-null once the bucket
                // is properly constructed for a defined FSM. Let's keep it nullable if there are transient null states,
                // but ensure it's handled.
                public FSM Definition;

                /// <summary>
                /// A list of all active <see cref="FSMHandle"/> instances created from this definition.
                /// </summary>
                /// <remarks>
                /// Instances are added to this list upon creation and removed upon unregistration
                /// or automatic removal due to error thresholds. The list itself is never null.
                /// </remarks>
                public List<FSMHandle> Instances = new List<FSMHandle>(); // Correct, initialized to a non-null empty list.

                /// <summary>
                /// The desired processing rate for instances of this FSM definition.
                /// </summary>
                /// <value>
                /// <c>-1</c>: Process every frame (or every call to <see cref="Interaction.Update(string)"/>).
                /// <c>0</c>: Event-driven only (will not be processed by <see cref="Interaction.Update(string)"/>'s internal tick).
                /// <c>&gt;0</c>: Process every Nth frame, where N is the value of <c>ProcessRate</c>.
                /// </value>
                public int ProcessRate; // int is non-nullable, correct.

                /// <summary>
                /// An internal counter used for frame-skipping when <see cref="ProcessRate"/> is greater than 0.
                /// </summary>
                public int Counter; // int is non-nullable, correct.
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
            /// Gets the total number of distinct processing groups (e.g., "Update", "FixedUpdate").
            /// </summary>
            public static int ProcessingGroupCount => _buckets.Count; // Correct.

            /// <summary>
            /// Sets a time limit (in milliseconds) for how long an FSM processing group update can take
            /// before a warning message is logged via OnInternalApiError. Helps identify performance bottlenecks.
            /// Default is 5ms.
            /// </summary>
            public static long TickPerformanceWarningThresholdMs { get; set; } = 5; // Correct.



            /// <summary>
            /// Gets the total number of unique FSM definitions across all processing groups.
            /// </summary>
            public static int TotalFsmDefinitionCount => _buckets.Sum(group => group.Value.Count); // Correct.

            /// <summary>
            /// Gets the total number of active FSM instances (handles) across all FSM definitions
            /// within all processing groups.
            /// </summary>
            public static int TotalFsmHandleCount => _buckets.Sum(
                group => group.Value.Sum(fsm => fsm.Value.Instances.Count)
            );



           

































            /// <summary>
            /// Destroys a complete FSM definition and all of its active instances.
            /// </summary>
            /// <remarks>
            /// This is a critical method for cleaning up the FSM system. It first gracefully
            /// shuts down all active instances and then removes the FSM definition itself
            /// from the API's internal registry.
            /// <para>
            /// It avoids the common "collection modified" error by first creating a temporary
            /// list of all instances to be destroyed, and then iterating over that new list.
            /// </para>
            /// </remarks>
            /// <param name="fsmDefinitionName">The unique name of the FSM definition to destroy.</param>
            /// <param name="processingGroup">The name of the processing group where the FSM is registered.</param>
            public static void DestroyFiniteStateMachine(string fsmDefinitionName, string processingGroup)
            {
                // Input validation.
                if (string.IsNullOrWhiteSpace(fsmDefinitionName))
                {
                    Error.InvokeInternalApiError("Cannot destroy FSM definition with a null or empty name.", null);
                    return;
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    Error.InvokeInternalApiError("Cannot destroy FSM definition from a null or empty processing group.", null);
                    return;
                }

                // Retrieve the FsmBucket containing the definition and its instances.
                // Use a try-get pattern to avoid exceptions if the group or FSM doesn't exist.
                if (!Internal.GetBuckets().TryGetValue(processingGroup, out var groupBuckets) ||
                    !groupBuckets.TryGetValue(fsmDefinitionName, out var fsmBucket))
                {
                    // The FSM doesn't exist, so there's nothing to destroy.
                    return;
                }

                // CRITICAL FIX: The `foreach` loop cannot modify the collection it is iterating over.
                // We create a temporary copy of the instances list to avoid the
                // `InvalidOperationException` that happens when we call DestroyInstance.
                var instancesToDestroy = fsmBucket.Instances.ToList();

                // Iterate over the temporary list and destroy each instance.
                // The DestroyInstance method will remove the handles from the ORIGINAL collection.
                foreach (var handle in instancesToDestroy)
                {
                    DestroyInstance(handle);
                }

                // After all instances are destroyed, we can safely remove the FSM bucket itself.
                groupBuckets.Remove(fsmDefinitionName);

                // If the group is now empty, clean it up.
                if (!groupBuckets.Any())
                {
                    Internal.GetBuckets().Remove(processingGroup);
                }

                // Reset the error count for this definition.
                Error.ResetDefinitionErrorCount(fsmDefinitionName);
            }

            /// <summary>
            /// Safely and efficiently destroys a single FSM instance using its handle.
            /// </summary>
            /// <remarks>
            /// This method directly indexes into the internal data structures using
            /// the FSMHandle's metadata to avoid inefficient and unsafe collection
            /// enumeration. It performs the necessary cleanup actions such as
            /// invoking the OnExit state logic and clearing error counts.
            /// </remarks>
            /// <param name="handle">The FSMHandle instance to destroy.</param>
            public static void DestroyInstance(FSMHandle handle)
            {
                if (handle == null)
                {
                    Error.InvokeInternalApiError("Cannot destroy a null FSMHandle instance.", null);
                    return;
                }
                
                try
                {
                    var bucket = Internal.GetBuckets()[handle.Definition.ProcessingGroup][handle.Definition.Name];
                    if (bucket.Instances.Remove(handle))
                    {
                        handle.DestroyHandle();
                        Error.GetErrorCounts().Remove(handle);
                    }
                    else
                    {
                        Error.InvokeInternalApiError(
                            $"FSMHandle '{handle.Name}' was not found in its expected bucket " +
                            $"'{handle.Definition.Name}' within processing group '{handle.Definition.ProcessingGroup}'. " +
                            "It may have already been unregistered.",
                            null
                        );
                    }
                }
                catch (KeyNotFoundException)
                {
                    Error.InvokeInternalApiError(
                        $"FSM definition or processing group for instance '{handle.Name}' could not be found. " +
                        "The instance may have already been unregistered.",
                        null
                    );
                }
            }


            /// <summary>
            /// Checks if a specific FSM definition exists within a given processing group.
            /// </summary>
            /// <param name="processingGroupName">The name of the processing group to check.</param>
            /// <param name="fsmName">The name of the FSM definition to look for.</param>
            /// <returns><c>true</c> if the FSM definition exists in the specified group; otherwise, <c>false</c>.</returns>
            public static bool DoesFsmDefinitionExist(string processingGroupName, string fsmName)
            {
                // Correct. TryGetValue makes 'group' non-null if true.
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.ContainsKey(fsmName);
                }
                return false;
            }


            /// <summary>
            /// Gets a flattened list of all active FSM Handles across all processing groups and FSM definitions.
            /// </summary>
            /// <returns>An enumerable collection of all active <see cref="FSMHandle"/> instances.</returns>
            public static IEnumerable<FSMHandle> GetAllFsmHandles()
            {
                return _buckets.SelectMany(group =>
                    group.Value.SelectMany(fsm => fsm.Value.Instances)
                );
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
            public static FsmBucket GetBucket(string fsmName, string processGroup)
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
                // 'groupBuckets' is non-nullable if TryGetValue returns true.
                if (_buckets.TryGetValue(processGroup, out Dictionary<string, FsmBucket> groupBuckets))
                {
                    // REMOVED: if(groupBuckets == null) { return null; }
                    // This check is redundant and incorrect as groupBuckets is guaranteed non-null here.

                    // Try to get the specific FsmBucket within that group
                    // 'fsmBucket' is non-nullable if TryGetValue returns true.
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


            /// <summary>
            /// Gets the internal collection of FSM processing buckets.
            /// </summary>
            /// <remarks>
            /// This accessor exposes the central internal data structure of the FSM system.
            /// It is intended for advanced internal diagnostics or specific integration scenarios
            /// and should be used with caution, respecting the single-threaded update model.
            /// </remarks>
            /// <returns>A nested dictionary representing the core FSM registry.</returns>
            public static Dictionary<string, Dictionary<string, FsmBucket>> GetBuckets()
            {
                return _buckets; // Correct.
            }


            /// <summary>
            /// Gets the number of FSM definitions within a specific processing group.
            /// </summary>
            /// <param name="processingGroupName">The name of the processing group.</param>
            /// <returns>The count of FSM definitions, or 0 if the group doesn't exist.</returns>
            public static int GetFsmDefinitionCountInGroup(string processingGroupName)
            {
                // Correct. TryGetValue makes 'group' non-null if true.
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.Count;
                }
                return 0;
            }


            /// <summary>
            /// Gets the names of all FSM definitions within a specific processing group.
            /// </summary>
            /// <param name="processingGroupName">The name of the processing group.</param>
            /// <returns>An enumerable of FSM names, or empty if the group doesn't exist.</returns>
            public static IEnumerable<string> GetFsmDefinitionNamesInGroup(string processingGroupName)
            {
                // Correct. TryGetValue makes 'group' non-null if true.
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.Keys;
                }
                return Enumerable.Empty<string>();
            }


            /// <summary>
            /// Retrieves a specific <see cref="FSMHandle"/> for a given FSM definition, context, and processing group.
            /// </summary>
            /// <remarks>
            /// This method provides a way to get a live handle to an FSM instance using its unique context object.
            /// It's a useful utility for systems that need to interact directly with a specific FSM instance
            /// and its state. The method ensures that all components (processing group, FSM definition, and instance)
            /// exist before returning a valid handle.
            /// </remarks>
            /// <param name="fsmDefinitionName">The name of the FSM definition to which the handle belongs.</param>
            /// <param name="context">The unique context object associated with the FSM instance.</param>
            /// <param name="processingGroup">The processing group the FSM instance is registered under.</param>
            /// <returns>
            /// The <see cref="FSMHandle"/> object if a match is found; otherwise, <c>null</c>.
            /// </returns>
            public static FSMHandle GetFSMHandle(string fsmDefinitionName, IStateContext context, string processingGroup)
            {
                // Check if the processing group exists first. This is a quick and safe exit.
                if (_buckets.TryGetValue(processingGroup, out var buckets))
                {
                    // Now, try to find the bucket for the specific FSM definition name.
                    if (buckets.TryGetValue(fsmDefinitionName, out var bucket))
                    {
                        // Finally, search for the FSMHandle with the matching context.
                        // Using FirstOrDefault is a good approach here.
                        var handle = bucket.Instances.FirstOrDefault(h => h.Context == context);

                        // If a handle is found, return it.
                        if (handle != null)
                        {
                            return handle;
                        }
                    }
                }

                // If any of the checks failed, or no matching handle was found, return null.
                // This is the "error" or "not found" condition you were missing.
                return null;
            }


            /// <summary>
            /// Gets the total number of active FSM instances (handles) within a specific processing group.
            /// </summary>
            /// <param name="processingGroupName">The name of the processing group.</param>
            /// <returns>The count of handles, or 0 if the group doesn't exist.</returns>
            public static int GetFSMHandleCountInGroup(string processingGroupName)
            {
                // Correct. TryGetValue makes 'group' non-null if true.
                if (_buckets.TryGetValue(processingGroupName, out var group))
                {
                    return group.Sum(fsm => fsm.Value.Instances.Count);
                }
                return 0;
            }


            /// <summary>
            /// Gets the names of all currently registered processing groups.
            /// </summary>
            /// <returns>An enumerable collection of processing group names.</returns>
            public static IEnumerable<string> GetProcessingGroupNames()
            {
                return _buckets.Keys;
            }


            /// <summary>
            /// Gets a list of all currently registered processing groups.
            /// </summary>
            /// <returns>A new <see cref="List{T}"/> containing the names of all processing groups.</returns>
            public static List<string> GetProcessingGroups()
            {
                return _buckets.Keys.ToList();
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
            public static void ProcessDeferredModifications()
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
            /// Resets the internal state of the FSM API.
            /// </summary>
            /// <param name="hardReset">
            /// If <c>true</c>, all FSM definitions and instances are completely cleared.
            /// If <c>false</c>, all existing FSM instances are unregistered and transitioned to their initial state,
            /// and all processing groups are cleared, effectively starting fresh without removing definitions.
            /// The deferred modifications queue and error counters are always cleared.
            /// </param>
            public static void ResetAPI(bool hardReset = false)
            {
                if (hardReset)
                {
                    _buckets.Clear();
                    Error.Reset();
                }
                else
                {
                    foreach (var processGroup in _buckets)
                    {
                        foreach (var fsmBucket in processGroup.Value)
                        {
                            foreach (var instance in fsmBucket.Value.Instances)
                            {
                                instance.DestroyHandle();
                            }
                            fsmBucket.Value.Instances.Clear();
                        }
                        processGroup.Value.Clear();
                    }
                    _buckets.Clear();
                }
                
            }



            /// <summary>
            /// Gets the internal default FSM definition.
            /// </summary>
            /// <returns>The static <see cref="FSM"/> object used as a default.</returns>
            internal static FSM GetDefaultFSM()
            {
                return _defaultFSM; // Correct.
            }


            /// <summary>
            /// Gets the queue of deferred modifications.
            /// </summary>
            /// <returns>The internal <see cref="Queue{T}"/> of <see cref="Action"/>s that are deferred.</returns>
            internal static Queue<Action> GetDeferred()
            {
                return _deferredModifications; // Correct.
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
            /// is not registered within that group. Returns the <see cref="_defaultFSM"/> if the requested FSM or its bucket definition is null.
            /// </returns>
            internal static FSM GetFSM(string name, string processingGroup = "Update")
            {
                // Step 1: Safely attempt to get the inner dictionary for the processing group.
                // Using TryGetValue prevents a KeyNotFoundException if the processingGroup doesn't exist.
                if (!_buckets.TryGetValue(processingGroup, out var groupMap))
                {
                    // The specified processing group does not exist.
                    // For internal methods, you might log this as a debug warning, but for a 'Get' method,
                    // returning null is the expected graceful failure.
                    // IMPORTANT: The return type is FSM?, but we're returning _defaultFSM here.
                    // This implies GetFSM will *never* truly return null if _defaultFSM is non-null.
                    // Let's align the comment to reflect this.
                    return _defaultFSM; // Change from null to _defaultFSM to match code logic.
                }

                // Step 2: Safely attempt to get the FsmBucket for the specific FSM name within that group.
                // Using TryGetValue prevents a KeyNotFoundException if the FSM name doesn't exist in the group.
                // 'groupMap' is non-nullable here.
                if (groupMap.TryGetValue(name, out var fsmBucket))
                {
                    // Both the group and the FSM were found. Return its Definition.
                    // fsmBucket.Definition is FSM?
                    // If it's null, return _defaultFSM. Otherwise return the non-null Definition.
                    // This means GetFSM itself will never return true `null` (only _defaultFSM)
                    return fsmBucket.Definition ?? _defaultFSM; // Simplified: Use null-coalescing operator.
                }
                else
                {
                    // The FSM with the given name was not found within the specified processing group.
                    return _defaultFSM; // Change from null to _defaultFSM to match code logic.
                }
            }


            /// <summary>
            /// Internally retrieves the dictionary of <see cref="FsmBucket"/>s for a given processing group.
            /// If the group does not exist, it will be created automatically.
            /// </summary>
            /// <remarks>
            /// This method is designed for **internal use** within the <see cref="FSM_API"/> assembly
            /// and is not part of the public API surface. It ensures that any requested processing group
            /// exists within the internal <c>_buckets</c> dictionary, creating it if necessary,
            /// thereby preventing <see cref="KeyNotFoundException"/> when attempting to add FSM definitions
            /// to a non-existent group.
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

                // Correct. TryGetValue makes 'categoryBuckets' non-null if true.
                if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets))
                {
                    categoryBuckets = new Dictionary<string, FsmBucket>();
                    _buckets[processingGroup] = categoryBuckets;
                    _processingGroupTickCounts[processingGroup] = 0;
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
            /// <param name="processingGroup">The processing group this FSM belongs to. Defaults to "Update".</param>
            /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.</exception>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="fsm"/> is null.</exception>
            internal static void Register(
                string fsmName,
                FSM fsm, // This 'fsm' parameter is non-nullable by signature, and checked. Correct.
                int processRate, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty during registration.", nameof(fsmName));
                }
                // Correct: null check for non-nullable parameter.
                if (fsm == null)
                {
                    throw new ArgumentNullException(nameof(fsm), "FSM definition cannot be null during registration.");
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty during registration.", nameof(processingGroup));
                }

                var buckets = GetOrCreateBucketProcessingGroup(processingGroup);
                // Correct. TryGetValue makes 'existingBucket' non-null if true.
                if (buckets.TryGetValue(fsmName, out var existingBucket))
                {
                    // FSM already exists, update its definition and process rate.
                    // Existing instances will automatically use the new definition on their next tick.
                    // Nullability: existingBucket.Definition is FSM?
                    // We're assigning a non-nullable 'fsm' to it, which is fine.
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
                        // Nullability: 'fsm' is non-nullable here, assigning to FsmBucket.Definition (FSM?) is fine.
                        Definition = fsm,
                        ProcessRate = processRate,
                        Counter = processRate > 0 ? processRate : 0,
                    };
                    Error.InvokeInternalApiError($"FSM '{fsmName}' in processing group '{processingGroup}' newly registered.", new Exception());
                }
            }




            /// <summary>
            /// Internally associates an FSM bucket with a specific processing group.
            /// </summary>
            /// <remarks>
            /// This method is for internal use when re-assigning an existing FsmBucket
            /// (which represents an FSM definition and its instances) to a new processing group.
            /// It ensures the target processing group exists and handles the removal from the old group.
            /// This assumes the bucket *already exists* in some group.
            /// </remarks>
            /// <param name="processGroup">The name of the target processing group.</param>
            /// <param name="bucket">The <see cref="FsmBucket"/> to associate with the processing group.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="bucket"/> is null.</exception>
            /// <exception cref="InvalidOperationException">Thrown if the <paramref name="bucket"/>'s
            /// <see cref="FsmBucket.Definition"/> is null, as a bucket must have a defined FSM name to be moved.</exception>
            /// <exception cref="KeyNotFoundException">Thrown if the bucket's definition name is not found in any existing processing group
            /// when attempting to remove it from its old group (this indicates an internal inconsistency).</exception>
            internal static void SetProcessGroup(string processGroup, FsmBucket bucket)
            {
                // Nullability: `bucket` parameter is non-nullable.
                if (bucket == null) // Explicit null check for clarity, though compiler might warn if you use it without one later.
                {
                    throw new ArgumentNullException(nameof(bucket), "FsmBucket cannot be null when setting its process group.");
                }

                if (!_buckets.ContainsKey(processGroup))
                {
                    // This method implicitly creates the group via GetOrCreateBucketProcessingGroup if needed.
                    // Create.CreateProcessingGroup is likely just a wrapper around GetOrCreateBucketProcessingGroup.
                    // If Create.CreateProcessingGroup can throw, you might want to consider that.
                    // Assuming it works as expected.
                    Create.CreateProcessingGroup(processGroup);
                }

                // Nullability: bucket.Definition is FSM?
                if (bucket.Definition == null)
                {
                    // If the bucket doesn't have a definition, it cannot be meaningfully moved/registered.
                    throw new InvalidOperationException("Cannot set process group for an FsmBucket with a null Definition.");
                }

                // Nullability: .First() could throw InvalidOperationException if no element matches.
                // .Value.ContainsKey(bucket.Definition.Name) ensures the nested dictionary contains the key.
                // .Value.Remove(bucket.Definition.Name) removes the item.
                // We need to find the CURRENT process group of the bucket.
                // This line looks for *any* process group that contains this bucket's definition name.
                // This could be problematic if the same definition name exists in multiple groups
                // (which is generally allowed if FSMs are unique *per group*).
                // If it's unique across *all* groups, then this logic is okay, but then `bucket.Definition.Name`
                // should probably be globally unique.

                // Let's assume for now that the intent is to remove it from its *current* group
                // wherever that might be.
                var currentGroupEntry = _buckets.FirstOrDefault(pg => pg.Value.ContainsKey(bucket.Definition.Name)); // Use ! since we just checked Definition for null
                if (currentGroupEntry.Value != null) // Check if FirstOrDefault found an entry
                {
                    currentGroupEntry.Value.Remove(bucket.Definition.Name);
                }
                else
                {
                    // This means the bucket was not found in any existing group, which might be an error state
                    // if this method is only called for already-registered buckets.
                    // Depending on desired behavior, you might throw an exception or log an error.
                    Error.InvokeInternalApiError($"SetProcessGroup: FsmBucket for '{bucket.Definition.Name}' not found in any existing processing group. Adding to '{processGroup}'.", null);
                }

                // Nullability: _buckets[processGroup] is Dictionary<string, FsmBucket> (non-nullable).
                // bucket.Definition.Name is guaranteed non-null here due to the check above.
                _buckets[processGroup].Add(bucket.Definition.Name, bucket);
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
            /// reported via <see cref="Error.InvokeInstanceError(FSMHandle, string, Exception, string)"/>, leading
            /// to automatic removal if error thresholds are exceeded.
            /// </remarks>
            internal static void TickAll(string processingGroup)
            {
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    // Correct: passing null for Exception?
                    Error.InvokeInternalApiError("TickAll called with null or empty processing group.", null);
                    return;
                }

                if (!_buckets.TryGetValue(processingGroup, out var fsmDefinitionsForCategory))
                {
                    return;
                }
                if(!_processingGroupTickCounts.ContainsKey(processingGroup))
                {
                    _processingGroupTickCounts.Add(processingGroup, 0);
                }
                _processingGroupTickCounts[processingGroup]++;
                // ToList() creates a copy, preventing collection modification errors during iteration.
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

                    var instancesToTick = bucket.Instances.ToList(); // Copy to avoid modification issues
                    foreach (var handle in instancesToTick)
                    {
                        // Ensure the handle itself is not null, its context is not null,
                        // AND its context is reported as valid by the context itself.
                        // `handle` is FSMHandle (non-nullable List element), but we need to ensure FSMHandle.Context
                        // is non-null before accessing its IsValid property.
                        if (handle != null && handle.Context != null && handle.Context.IsValid)
                        {
                            try
                            {
                                if (!handle.HasEnteredCurrentState)
                                {
                                    bucket.Definition.GetState(bucket.Definition.InitialState).Enter(handle.Context);
                                    handle.HasEnteredCurrentState = true;
                                }
                                bucket.Definition.GetState(handle.CurrentState).Update(handle.Context);

                            }
                            catch (Exception ex)
                            {
                                Error.InvokeInstanceError(handle,
                                    $"Error during FSM instance '{handle.Name}' update in state '{handle.CurrentState}'.", // Added a descriptive message
                                    ex, processingGroup);
                            }
                        }
                        else
                        {
                            Error.InvokeInstanceError(handle,
                                "FSM instance or its context became null/invalid (IsValid returned false). Automatically shutting down due to instability.",
                                new ApplicationException("FSM instance or its context became null/invalid (IsValid returned false)."), processingGroup);

                        }
                    }
                }
            }

            /// <summary>
            /// Provides Internal access to a processing group's tick count
            /// </summary>
            /// <param name="processingGroup"></param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public static ulong GetProcessingGroupTickCount(string processingGroup)
            {
                if(_processingGroupTickCounts.TryGetValue(processingGroup, out var tickCount))
                {
                    return tickCount;
                }
                return 0;
            }


            /// <summary>
            /// A queue of actions representing modifications to the FSM system
            /// that are deferred to prevent collection modification errors during iteration.
            /// </summary>
            /// <remarks>
            /// Operations like adding or removing FSM instances/definitions are
            /// enqueued and processed at safe points in the update cycle (e.g., after all FSMs have ticked).
            /// The queue itself is static and initialized, thus never null.
            /// </remarks>
            private static readonly Queue<Action> _deferredModifications = new Queue<Action>();

            /// <summary>
            /// Stores all FSM definitions and their instances, organized by
            /// their processing group (e.g., "Update", "FixedUpdate") and then by FSM name.
            /// </summary>
            /// <remarks>
            /// This nested dictionary forms the central registry for the FSM system,
            /// allowing efficient lookup and management of FSM blueprints and their live
            /// manifestations. Access to this collection should be strictly confined to the single
            /// designated thread where FSM updates occur. The dictionary itself is never null.
            /// </remarks>
            private static Dictionary<string, Dictionary<string, FsmBucket>> _buckets = new Dictionary<string, Dictionary<string, FsmBucket>>(); // Correct, initialized.


            /// <summary>
            /// for tracking the tick counts for each processing group.
            /// </summary>
            private static Dictionary<string, ulong> _processingGroupTickCounts = new Dictionary<string, ulong>();


            /// <summary>
            /// Represents a default FSM definition for internal API operations.
            /// </summary>
            /// <remarks>
            /// This default instance is used in scenarios where an expected FSM definition
            /// is not found or is null, providing a fallback to prevent NullReferenceExceptions
            /// while allowing the system to continue functioning with a safe, empty FSM.
            /// </remarks>
            private static FSM _defaultFSM = new FSM(); // Correct, initialized.
        }
    }

}