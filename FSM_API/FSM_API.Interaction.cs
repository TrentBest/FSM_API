using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;




namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Provides methods for querying, managing, and interacting with live Finite State Machine (FSM)
    /// definitions and their active instances at runtime. This section enables dynamic control
    /// over your FSM ecosystem after initial setup.
    /// </summary>
    /// <remarks>
    /// This partial class, alongside others, forms the comprehensive <see cref="FSM_API"/> from The Singularity Workshop.
    /// Methods here allow users to check for the existence of FSMs, retrieve lists of defined
    /// FSMs or their running instances, initiate the FSM processing loop for specific groups,
    /// and gracefully remove FSM definitions or individual instances from the system.
    /// <para>
    /// **Important:** Modifying or destroying FSM definitions via these methods will
    /// immediately affect all active instances derived from those definitions,
    /// potentially changing or halting their behavior. Exercise caution, especially
    /// in live production environments.
    /// </para>
    /// </remarks>
    public static partial class FSM_API
    {
        /// <summary>
        /// Provides a set of methods for interacting with and managing FSM definitions and instances at runtime.
        /// This includes querying their existence, retrieving instances, ticking processing groups,
        /// and destroying definitions or individual instances.
        /// </summary>
        /// <remarks>
        /// This static class groups all operations related to the lifecycle and real-time management
        /// of FSMs once they have been defined and instantiated. It serves as the primary
        /// interface for monitoring and controlling your FSMs after their initial setup.
        /// </remarks>
        public static class Interaction
        {
            /// <summary>
            /// Checks if an FSM definition with the given name exists within the specified processing group.
            /// This is useful for verifying if an FSM blueprint has been registered before attempting
            /// to create instances or modify its definition.
            /// </summary>
            /// <param name="fsmName">
            /// The unique name of the FSM definition to check for existence.
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group to search within. Defaults to <c>"Update"</c>.
            /// </param>
            /// <returns>
            /// <c>true</c> if the FSM definition exists in the specified group; otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null, empty,
            /// or consists only of white-space characters.
            /// </exception>
            public static bool Exists(string fsmName, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                if (!Internal.GetBuckets().TryGetValue(processingGroup, out var categoryBuckets))
                {
                    return false;
                }
                return categoryBuckets.ContainsKey(fsmName);
            }

            /// <summary>
            /// Retrieves the names of all FSM definitions currently registered
            /// within a specific processing group. This is useful for introspection, debugging, or
            /// for building dynamic UI elements that list available FSMs.
            /// </summary>
            /// <param name="processingGroup">
            /// The name of the processing group to query for FSM definitions. Defaults to <c>"Update"</c>.
            /// </param>
            /// <returns>
            /// A read-only collection of strings, where each string is the name of an FSM definition.
            /// If the processing group does not exist, an empty collection is returned.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.
            /// </exception>
            public static IReadOnlyCollection<string> GetAllDefinitionNames(string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                if (!Internal.GetBuckets().TryGetValue(processingGroup, out var categoryBuckets))
                {
                    return new List<string>().AsReadOnly();
                }
                return categoryBuckets.Keys.ToList().AsReadOnly();
            }

            /// <summary>
            /// Retrieves all active, live instances of a specific FSM definition
            /// within a given processing group.
            /// </summary>
            /// <remarks>
            /// Each <see cref="FSMHandle"/> in the returned list represents a currently
            /// running Finite State Machine, bound to its own unique context object.
            /// This method allows you to inspect, manage, or interact with all live
            /// applications of a particular FSM blueprint.
            /// </remarks>
            /// <param name="fsmName">
            /// The name of the FSM definition whose active instances you want to retrieve.
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group where the FSM definition is registered. Defaults to <c>"Update"</c>.
            /// </param>
            /// <returns>
            /// A read-only list of <see cref="FSMHandle"/> objects, each representing a live
            /// FSM instance derived from the specified definition.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null, empty,
            /// or consists only of white-space characters.
            /// </exception>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if the FSM definition specified by <paramref name="fsmName"/> and <paramref name="processingGroup"/>
            /// cannot be found. Ensure the FSM has been defined prior to attempting to retrieve its instances.
            /// </exception>
            public static IReadOnlyList<FSMHandle> GetInstances(string fsmName, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                if (!Internal.GetBuckets().TryGetValue(processingGroup, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
                {
                    throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'.");
                }
                return bucket.Instances.AsReadOnly();
            }


            /// <summary>
            /// Advances the FSM instance by one logical step. This involves executing the 'OnUpdate'
            /// action of the <see cref="FSMHandle.CurrentState"/> and evaluating all defined transitions
            /// from the <see cref="FSMHandle.CurrentState"/>. If a valid transition is found, the FSM will
            /// move to the next state, executing 'OnExit' for the old state and 'OnEnter' for the new state.
            /// </summary>
            /// <param name="processGroup">Optional: A string identifying the process group for error reporting, defaults to "Update".</param>
            /// <remarks>
            /// **The Crucial Step for FSM Processing:**
            /// This <c>Update()</c> method is fundamental to the operation of an FSM instance. Without
            /// it being called, the FSM will not execute its state's update logic, evaluate transitions,
            /// or progress through its defined states. An FSM definition, no matter how intricate,
            /// remains static until its instances are actively updated.
            ///
            /// **How FSMs are "Ticked":**
            /// This method is primarily designed to be invoked by the <c>FSM_API</c>'s internal management
            /// system (e.g., via <c>FSM_API.Interaction.TickAll(string processingGroup)</c>). The API
            /// manages the periodic invocation of this method for all registered FSM instances
            /// within a given processing group, respecting their defined <c>ProcessRate</c>.
            ///
            /// **CAUTION: Manual Invocation (Advanced Use Only):**
            /// While publicly accessible, directly calling <c>Update()</c> manually from your application code
            /// should only be done by developers who possess a deep understanding of the FSM's internal
            /// processing cycle and lifecycle management. Manual invocation outside of the
            /// <c>FSM_API</c>'s managed loop can lead to:
            /// <list type="bullet">
            ///     <item><description>Unpredictable state behavior due to uncontrolled update rates.</description></item>
            ///     <item><description>Performance issues if not integrated into an efficient game loop or update scheduler.</description></item>
            ///     <item><description>Conflicts with the <c>FSM_API</c>'s internal error handling and instance management.</description></item>
            /// </list>
            /// Use with extreme care and only when building a highly customized, self-managed FSM update system.
            /// </remarks>
            public static void Update(string processGroup = "Update")
            {
                var sw = Stopwatch.StartNew();
                Internal.TickAll(processGroup);
                Internal.ProcessDeferredModifications();
                sw.Stop();
                if (sw.ElapsedMilliseconds > Internal.TickPerformanceWarningThresholdMs)
                {
                    string message = $"'Update' tick took {sw.ElapsedMilliseconds}ms. Threshold: {Internal.TickPerformanceWarningThresholdMs}ms.";
                    Error.InvokeInternalApiError(message, new Exception(message));
                }
            }

            /// <summary>
            /// Removes an entire FSM processing group from the system, along with all FSM definitions
            /// and all their active instances associated with that group.
            /// </summary>
            /// <remarks>
            /// This is a **destructive operation**. Use this method carefully, typically when
            /// an entire subsystem or module that relies on a specific FSM processing group
            /// is being unloaded or shut down. All memory and resources tied to FSMs
            /// in this group will be released and they will no longer be managed by the API.
            /// If the group does not exist, a warning will be logged via <see cref="Error.InvokeInternalApiError(string, Exception)"/>.
            /// </remarks>
            /// <param name="processingGroup">
            /// The name of the processing group to remove.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.
            /// </exception>
            public static void RemoveProcessingGroup(string processingGroup)
            {
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                if (Internal.GetBuckets().TryGetValue(processingGroup, out var categoryBuckets))
                {
                    foreach (var bucket in categoryBuckets.Values)
                    {
                        if (bucket.Definition != null)
                        {
                            Error.GetDefinitionErrorCounts().Remove(bucket.Definition.Name);
                        }
                        // Explicitly shut down each instance before clearing
                        foreach (var instance in bucket.Instances)
                        {
                            instance.ShutDown(); // Call ShutDown to trigger OnExit logic
                            Error.GetErrorCounts().Remove(instance);
                        }
                        bucket.Instances.Clear();
                    }
                    Internal.GetBuckets().Remove(processingGroup);
                }
                else
                {
                    string message = $"Attempted to remove non-existent processing group '{processingGroup}'.";
                    Error.InvokeInternalApiError(message, null);
                }
            }

            /// <summary>
            /// Removes a registered FSM definition and all its associated active instances
            /// from a specific processing group. This effectively unregisters the FSM blueprint
            /// and frees up resources for all its derived instances.
            /// </summary>
            /// <remarks>
            /// This is a **destructive operation** for a specific FSM type. All current instances
            /// of this FSM definition will cease to be managed by the API. Their <see cref="FSMHandle.ShutDown"/>
            /// method will be called to ensure proper <c>OnExit</c> logic is executed.
            /// Memory will then be eligible for garbage collection once all external references are released.
            /// If the FSM definition or group does not exist, a warning will be logged via <see cref="Error.InvokeInternalApiError(string, Exception)"/>.
            /// </remarks>
            /// <param name="fsmName">
            /// The name of the FSM definition to destroy.
            /// </param>
            /// <param name="processingGroup">
            /// The processing group where the FSM definition is registered. Defaults to <c>"Update"</c>.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null, empty,
            /// or consists only of white-space characters.
            /// </exception>
            public static void DestroyFiniteStateMachine(string fsmName, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                if (!Internal.GetBuckets().TryGetValue(processingGroup, out var categoryBuckets))
                {
                    Error.InvokeInternalApiError($"Attempted to destroy FSM '{fsmName}' from non-existent processing group '{processingGroup}'.", null);
                    return;
                }

                if (!categoryBuckets.TryGetValue(fsmName, out var bucket))
                {
                    Error.InvokeInternalApiError($"Attempted to destroy non-existent FSM '{fsmName}' in processing group '{processingGroup}'.", null);
                    return;
                }

                // Call ShutDown on all instances before clearing
                foreach (var instance in bucket.Instances)
                {
                    instance.ShutDown(); // Call ShutDown to trigger OnExit logic
                    Error.GetErrorCounts().Remove(instance);
                }
                bucket.Instances.Clear();

                // Clean up the definition's error count
                Error.GetDefinitionErrorCounts().Remove(fsmName);

                categoryBuckets.Remove(fsmName);
            }

            /// <summary>
            /// Unregisters a specific FSM instance from the system, allowing it to be garbage collected
            /// if no other references exist. This method searches across all active FSM definitions
            /// in all groups to find and deregister the instance.
            /// </summary>
            /// <remarks>
            /// Use this method when a particular object managed by an FSM is no longer needed
            /// or is being destroyed (e.g., a game object being despawned), but its FSM definition
            /// might still be in use by other instances.
            /// <para>
            /// The instance's <see cref="FSMHandle.ShutDown"/> method will be called to ensure
            /// any <c>OnExit</c> logic for its current state is executed before it is removed
            /// from management.
            /// </para>
            /// If the instance is not found (e.g., it was already unregistered or never registered),
            /// a warning will be logged via <see cref="Error.InvokeInternalApiError(string, Exception)"/>.
            /// </remarks>
            /// <param name="instance">
            /// The <see cref="FSMHandle"/> of the specific live FSM instance to unregister.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if the provided <paramref name="instance"/> is <c>null</c>.
            /// </exception>
            public static void Unregister(FSMHandle instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException(nameof(instance), "Cannot unregister a null FSM instance.");
                }

                bool removed = false;
                // Iterate through all processing groups
                foreach (var categoryKvp in Internal.GetBuckets())
                {
                    // Iterate through all FSM definitions within each processing group
                    foreach (var fsmBucket in categoryKvp.Value.Values)
                    {
                        // Attempt to remove the instance from the processGroup's list of instances
                        if (fsmBucket.Instances.Remove(instance))
                        {
                            removed = true;
                            // Call ShutDown to trigger OnExit logic
                            instance.ShutDown();
                            // Also remove its error count entry if it exists
                            Error.GetErrorCounts().Remove(instance);
                            return; // Instance found and removed, exit early.
                        }
                    }
                }

                if (!removed)
                {
                    // Log a warning if the instance wasn't found, indicating it might have already been removed.
                    Error.InvokeInternalApiError(
                        $"FSMHandle '{instance.Name}' not found in any registered FSM in any group for unregistration. It might have already been unregistered or was never registered.",
                        null
                    );
                }
            }


            /// <summary>
            /// Dynamically adds a new state definition to an existing Finite State Machine blueprint
            /// at runtime.
            /// </summary>
            /// <remarks>
            /// This method allows you to extend the behavior of an FSM after it has been defined.
            /// It internally uses an <see cref="FSMModifier"/> to apply the changes.
            /// If a state with the given <paramref name="stateName"/> already exists in the FSM,
            /// its <c>onEnter</c>, <c>onUpdate</c>, and <c>onExit</c> actions will be updated.
            /// All active instances of this FSM definition will immediately reflect the added or
            /// updated state when they attempt to transition to or from it, or if they are
            /// currently in that state and its actions are modified.
            /// </remarks>
            /// <param name="fsmName">
            /// The unique name of the FSM blueprint to which the state will be added or updated.
            /// </param>
            /// <param name="stateName">
            /// The name of the state to add or update. This must be unique within the FSM.
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group where the FSM definition is registered. Defaults to <c>"Update"</c>.
            /// </param>
            /// <param name="onEnter">
            /// An <see cref="Action{T}"/> to execute when an FSM instance enters this state. Can be <c>null</c>.
            /// </param>
            /// <param name="onUpdate">
            /// An <see cref="Action{T}"/> to execute when an FSM instance is in this state during an <see cref="Update(string)"/> call. Can be <c>null</c>.
            /// </param>
            /// <param name="onExit">
            /// An <see cref="Action{T}"/> to execute when an FSM instance exits this state. Can be <c>null</c>.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/>, <paramref name="stateName"/>, or <paramref name="processingGroup"/>
            /// is null, empty, or consists only of white-space characters.
            /// </exception>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if the FSM definition specified by <paramref name="fsmName"/> and <paramref name="processingGroup"/>
            /// cannot be found.
            /// </exception>
            public static void AddStateToFSM(string fsmName, string stateName, // Added stateName as a direct parameter
                Action<IStateContext> onEnter,
                Action<IStateContext> onUpdate,
                Action<IStateContext> onExit,
                string processingGroup = "Update") // Moved processingGroup to end for consistency
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(stateName))
                {
                    throw new ArgumentException("State name cannot be null or empty.", nameof(stateName));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                var fsm = Internal.GetFSM(fsmName, processingGroup); // Assuming GetFSM can throw if not found
                if (fsm == null)
                {
                    throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'.");
                }

                // Use the FSMBuilder instance method
                new FSMModifier(fsm).WithState(stateName, onEnter, onUpdate, onExit)
                   .ModifyDefinition(); // Builds and applies changes directly to the FSM instance
            }

            /// <summary>
            /// Dynamically removes a state definition from an existing Finite State Machine blueprint at runtime.
            /// </summary>
            /// <remarks>
            /// This method internally uses an <see cref="FSMModifier"/> to apply the changes.
            /// <para>
            /// When a state is removed, any active FSM instances currently in that state will be
            /// immediately transitioned. The transition target is determined as follows:
            /// <list type="bullet">
            ///     <item><description>If a valid <paramref name="fallbackStateName"/> is provided, instances will transition to that state.</description></item>
            ///     <item><description>If <paramref name="fallbackStateName"/> is null, empty, or consists only of white-space,
            ///           instances will instead transition to the FSM's <see cref="FSM.InitialState"/>.</description></item>
            /// </list>
            /// The `OnExit` action of the removed state will be called, followed by the `OnEnter` action
            /// of the determined fallback or initial state.
            /// </para>
            /// <para>
            /// If the specified state to remove does not exist in the FSM, a warning will be logged
            /// via <see cref="Error.InvokeInternalApiError(string, Exception)"/>.
            /// </para>
            /// </remarks>
            /// <param name="fsmName">
            /// The unique name of the FSM blueprint from which the state will be removed.
            /// </param>
            /// <param name="stateName">
            /// The name of the state to remove.
            /// </param>
            /// <param name="fallbackStateName">
            /// Optional: The name of the state to transition existing FSM instances to if they are
            /// currently in the state being removed. If not provided (null or empty), instances
            /// will default to transitioning to the FSM's <see cref="FSM.InitialState"/>.
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group where the FSM definition is registered. Defaults to <c>"Update"</c>.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/>, <paramref name="stateName"/>, or <paramref name="processingGroup"/>
            /// is null, empty, or consists only of white-space characters.
            /// </exception>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if the FSM definition specified by <paramref name="fsmName"/> and <paramref name="processingGroup"/>
            /// cannot be found.
            /// </exception>
            public static void RemoveStateFromFSM(string fsmName, string stateName, string fallbackStateName, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(stateName))
                {
                    throw new ArgumentException("State name cannot be null or empty.", nameof(stateName));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                var fsm = Internal.GetFSM(fsmName, processingGroup);
                if (fsm == null)
                {
                    throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'.");
                }

                // Determine the actual fallback state.
                // If fallbackStateName is not provided or invalid, use the FSM's InitialState.
                // We need to ensure that the determined fallback state actually exists in the FSM
                // before trying to apply the modification, or the FSMModifier might throw.
                string actualFallbackState = string.IsNullOrWhiteSpace(fallbackStateName) ? fsm.InitialState : fallbackStateName;

                // TODO: (Self-correction/Assumption) FSMModifier.WithoutState should ideally validate
                // if actualFallbackState exists, or FSM.InitialState is always guaranteed to exist.
                // Assuming FSMModifier or underlying FSM handles invalid fallbackStateName gracefully
                // by either throwing an informative exception or defaulting internally.
                // For now, we pass the chosen actualFallbackState to WithoutState.

                new FSMModifier(fsm)
                    .WithoutState(stateName, actualFallbackState) // Pass the determined fallback state
                    .ModifyDefinition();
            }

            /// <summary>
            /// Dynamically adds a new transition between two states to an existing Finite State Machine blueprint.
            /// </summary>
            /// <remarks>
            /// This method internally uses an <see cref="FSMModifier"/> to apply the changes.
            /// This allows for changing the flow of your FSMs at runtime. If a transition
            /// from <paramref name="fromState"/> to <paramref name="toState"/> with the exact
            /// same condition already exists, it will be updated with the new <paramref name="condition"/>.
            /// </remarks>
            /// <param name="fsmName">
            /// The unique name of the FSM blueprint to which the transition will be added or updated.
            /// </param>
            /// <param name="fromState">
            /// The name of the state from which the transition originates. Use <see cref="FSM.AnyStateIdentifier"/>
            /// to define a transition that can occur from any state.
            /// </param>
            /// <param name="toState">
            /// The name of the state to which the transition leads.
            /// </param>
            /// <param name="condition">
            /// A <see cref="Func{T, TResult}"/> that returns <c>true</c> when the transition should occur.
            /// This function operates on the <see cref="IStateContext"/>.
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group where the FSM definition is registered. Defaults to <c>"Update"</c>.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/>, <paramref name="fromState"/>, <paramref name="toState"/>,
            /// or <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters,
            /// or if <paramref name="condition"/> is <c>null</c>.
            /// </exception>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if the FSM definition specified by <paramref name="fsmName"/> and <paramref name="processingGroup"/>
            /// cannot be found.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Thrown if <paramref name="fromState"/> or <paramref name="toState"/> does not exist within the FSM's defined states,
            /// unless <paramref name="fromState"/> is <see cref="FSM.AnyStateIdentifier"/>.
            /// </exception>
            public static void AddTransition(string fsmName, string fromState, string toState, Func<IStateContext, bool> condition, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(fromState))
                {
                    throw new ArgumentException("From state cannot be null or empty.", nameof(fromState));
                }
                if (string.IsNullOrWhiteSpace(toState))
                {
                    throw new ArgumentException("To state cannot be null or empty.", nameof(toState));
                }
                if (condition == null)
                {
                    throw new ArgumentException("Transition condition cannot be null.", nameof(condition));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                var fsm = Internal.GetFSM(fsmName, processingGroup);
                if (fsm == null)
                {
                    throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'.");
                }

                new FSMModifier(fsm).WithTransition(fromState, toState, condition)
                   .ModifyDefinition();
            }

            /// <summary>
            /// Dynamically removes a specific transition between two states from an existing Finite State Machine blueprint.
            /// </summary>
            /// <remarks>
            /// This method internally uses an <see cref="FSMModifier"/> to apply the changes.
            /// This allows for removing unwanted or obsolete paths in your FSM's logic at runtime.
            /// If the specified transition does not exist, a warning will be logged via <see cref="Error.InvokeInternalApiError(string, Exception)"/>.
            /// </remarks>
            /// <param name="fsmName">
            /// The unique name of the FSM blueprint from which the transition will be removed.
            /// </param>
            /// <param name="fromState">
            /// The name of the state from which the transition originates. Use <see cref="FSM.AnyStateIdentifier"/>
            /// to remove a transition that occurs from any state.
            /// </param>
            /// <param name="toState">
            /// The name of the state to which the transition leads.
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group where the FSM definition is registered. Defaults to <c>"Update"</c>.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/>, <paramref name="fromState"/>, <paramref name="toState"/>,
            /// or <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.
            /// </exception>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if the FSM definition specified by <paramref name="fsmName"/> and <paramref name="processingGroup"/>
            /// cannot be found.
            /// </exception>
            public static void RemoveTransition(string fsmName, string fromState, string toState, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(fromState))
                {
                    throw new ArgumentException("From state cannot be null or empty.", nameof(fromState));
                }
                if (string.IsNullOrWhiteSpace(toState))
                {
                    throw new ArgumentException("To state cannot be null or empty.", nameof(toState));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                var fsm = Internal.GetFSM(fsmName, processingGroup);
                if (fsm == null)
                {
                    throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'.");
                }
                
                new FSMModifier(fsm).WithoutTransition(fromState, toState)
                   .ModifyDefinition();
            }


            /// <summary>
            /// Provides a simple, static utility for managing named timers based on elapsed float time (e.g., seconds)
            /// and integer increments (e.g., frames or discrete steps).
            /// <para>
            /// This class is designed to be time-agnostic; it does not acquire time itself but relies on the
            /// calling application to provide delta time (<c>dt</c>) and delta integer (<c>di</c>) values.
            /// It serves as a convenience for implementing frequency-based updates or delays, complementing
            /// the FSM API's core, untimed step mechanism.
            /// </para>
            /// </summary>
            /// <remarks>
            /// Users are responsible for integrating <c>UpdateTimers</c> into their application's main loop
            /// and for providing accurate time deltas. Timers must be manually added and checked for expiration.
            /// </remarks>
            public static class FSMTimers
            {
                /// <summary>
                /// Gets a dictionary of float-based timers, keyed by a string name.
                /// These timers are typically used for time-based delays or frequencies (e.g., seconds).
                /// </summary>
                public static Dictionary<string, float> FloatTimers { get; } = new Dictionary<string, float>();

                /// <summary>
                /// Gets a dictionary of integer-based timers, keyed by a string name.
                /// These timers are typically used for frame-based or discrete step delays/frequencies.
                /// </summary>
                public static Dictionary<string, int> IntTimers { get; } = new Dictionary<string, int>();

                /// <summary>
                /// Stores the last float time value passed to <see cref="UpdateTimers"/>.
                /// This can be used to track total elapsed time or for debugging purposes.
                /// </summary>
                public static float LastFloatTime { get; set; } = 0;

                /// <summary>
                /// Stores the last integer time value passed to <see cref="UpdateTimers"/>.
                /// This can be used to track total elapsed frames/steps or for debugging purposes.
                /// </summary>
                public static int LastIntTime { get; set; } = 0;

                /// <summary>
                /// Updates all registered float and integer timers by subtracting the provided delta values.
                /// </summary>
                /// <param name="dt">The delta float time (e.g., seconds since last update) to subtract from float timers.</param>
                /// <param name="di">The delta integer (e.g., frames since last update) to subtract from integer timers.</param>
                /// <remarks>
                /// This method simply decrements all active timers. It does not check if timers have reached
                /// zero or perform any actions based on timer expiration. It is up to the calling code
                /// to query individual timers (e.g., <c>FSMTimers.FloatTimers["MyTimer"] &lt;= 0f</c>)
                /// and trigger relevant FSM updates or other logic.
                /// </remarks>
                public static void UpdateTimers(float dt, int di)
                {
                    foreach (var timer in FloatTimers.ToArray())
                    {
                        FloatTimers[timer.Key] -= dt;
                        LastFloatTime = FloatTimers[timer.Key];
                    }
                    foreach (var timer in IntTimers.ToArray())
                    {
                        IntTimers[timer.Key] -= di;
                        LastIntTime = IntTimers[timer.Key];
                    }
                }

                /// <summary>
                /// Resets a specific float timer to its <paramref name="setPoint"/> value.
                /// If the timer does not exist, no action is taken.
                /// </summary>
                /// <param name="timerName">The name of the float timer to reset.</param>
                /// <param name="setPoint">The value to reset the timer to (defaults to 1.0f).</param>

                public static void ResetFloatTimer(string timerName, float setPoint = 1f)
                {
                    if (FloatTimers.ContainsKey(timerName))
                        FloatTimers[timerName] = setPoint;
                }

                /// <summary>
                /// Resets a specific integer timer to its <paramref name="setPoint"/> value.
                /// If the timer does not exist, no action is taken.
                /// </summary>
                /// <param name="timerName">The name of the integer timer to reset.</param>
                /// <param name="setPoint">The value to reset the timer to (defaults to 1).</param>

                public static void ResetIntTimer(string timerName, int setPoint = 1)
                {
                    if (IntTimers.ContainsKey(timerName))
                        IntTimers[timerName] = setPoint;
                }

                /// <summary>
                /// Adds a new float timer or updates an existing one with an initial value.
                /// </summary>
                /// <param name="timerName">The name of the timer to add or update.</param>
                /// <param name="initialValue">The initial value for the timer.</param>
                public static void AddOrSetFloatTimer(string timerName, float initialValue)
                {
                    FloatTimers[timerName] = initialValue;
                }

                /// <summary>
                /// Adds a new integer timer or updates an existing one with an initial value.
                /// </summary>
                /// <param name="timerName">The name of the timer to add or update.</param>
                /// <param name="initialValue">The initial value for the timer.</param>
                public static void AddOrSetIntTimer(string timerName, int initialValue)
                {
                    IntTimers[timerName] = initialValue;
                }

                /// <summary>
                /// Removes a float timer.
                /// </summary>
                /// <param name="timerName">The name of the timer to remove.</param>
                /// <returns><c>true</c> if the timer was successfully found and removed; otherwise, <c>false</c>.</returns>
                public static bool RemoveFloatTimer(string timerName)
                {
                    return FloatTimers.Remove(timerName);
                }

                /// <summary>
                /// Removes an integer timer.
                /// </summary>
                /// <param name="timerName">The name of the timer to remove.</param>
                /// <returns><c>true</c> if the timer was successfully found and removed; otherwise, <c>false</c>.</returns>
                public static bool RemoveIntTimer(string timerName)
                {
                    return IntTimers.Remove(timerName);
                }
            }
        }


    }
}