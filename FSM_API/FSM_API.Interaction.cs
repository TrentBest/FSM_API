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
            /// Processes all FSM instances associated with the specified processing group.
            /// This method typically advances the state of tick-based FSMs and processes
            /// deferred modifications to FSM definitions.
            /// </summary>
            /// <remarks>
            /// This method should be called regularly (e.g., once per frame in a game loop,
            /// or in a fixed-rate timer) to ensure that FSMs with a defined <see cref="FSMBuilder.WithProcessRate(int)"/>
            /// of greater than zero (`&gt; 0`) or `-1` (manual tick) update their internal logic.
            /// Purely event-driven FSMs do not strictly require this method for immediate transitions,
            /// but calling it ensures any deferred changes to FSM definitions are applied.
            /// <para>
            /// A performance warning will be logged via <see cref="Error.InvokeInternalApiError(string, Exception)"/>
            /// if the processing time for this tick cycle exceeds <see cref="Error.TickPerformanceWarningThresholdMs"/>.
            /// This helps in identifying potential performance bottlenecks in your FSM logic.
            /// </para>
            /// </remarks>
            /// <param name="processGroup">
            /// The name of the processing group whose FSMs should be updated. Defaults to <c>"Update"</c>.
            /// </param>
            public static void Update(string processGroup = "Update")
            {
                var sw = Stopwatch.StartNew();
                Internal.TickAll(processGroup);
                Internal.ProcessDeferredModifications();
                sw.Stop();
                if (sw.ElapsedMilliseconds > Error.TickPerformanceWarningThresholdMs)
                {
                    Error.InvokeInternalApiError($"'Update' tick took {sw.ElapsedMilliseconds}ms. Threshold: {Error.TickPerformanceWarningThresholdMs}ms.", null);
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
                    Error.InvokeInternalApiError($"Attempted to remove non-existent processing group '{processingGroup}'.", null);
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
                new FSMBuilder(fsm).State(stateName, onEnter, onUpdate, onExit)
                   .BuildDefinition(); // Builds and applies changes directly to the FSM instance
            }

            /// <summary>
            /// Dynamically removes a state definition from an existing Finite State Machine blueprint at runtime.
            /// </summary>
            /// <remarks>
            /// When a state is removed, any active FSM instances currently in that state will be
            /// immediately transitioned to a specified <paramref name="fallbackStateName"/>.
            /// If no fallback state is provided, instances will be transitioned to the FSM's
            /// initial state. The `OnExit` action of the removed state will be called, followed
            /// by the `OnEnter` action of the fallback or initial state.
            /// <para>
            /// If the specified state does not exist, a warning will be logged via <see cref="Error.InvokeInternalApiError(string, Exception)"/>.
            /// </para>
            /// </remarks>
            /// <param name="fsmName">
            /// The unique name of the FSM blueprint from which the state will be removed.
            /// </param>
            /// <param name="stateName">
            /// The name of the state to remove.
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
            public static void RemoveStateFromFSM(string fsmName, string stateName, string processingGroup = "Update")
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

                new FSMBuilder(fsm).Without(stateName)
                   .BuildDefinition();
            }

            /// <summary>
            /// Dynamically adds a new transition between two states to an existing Finite State Machine blueprint.
            /// </summary>
            /// <remarks>
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

                new FSMBuilder(fsm).Transition(fromState, toState, condition)
                   .BuildDefinition();
            }

            /// <summary>
            /// Dynamically removes a specific transition between two states from an existing Finite State Machine blueprint.
            /// </summary>
            /// <remarks>
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

                new FSMBuilder(fsm).WithoutTransition(fromState, toState)
                   .BuildDefinition();
            }
        }
    }
}