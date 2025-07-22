using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSingularityWorkshop.FSM_API
{
    public static partial class FSM_API
    {
        public static class Error
        {
            private static readonly Dictionary<FSMHandle, int> _errorCounts = new();

            /// <summary>
            /// Tracks the cumulative number of instance-level errors that have occurred
            /// for each FSM definition, leading to automatic instance unregistration.
            /// Key: FSM Definition Name, Value: Cumulative Error Count.
            /// </summary>
            /// <remarks>
            /// Used in conjunction with <see cref="DefinitionErrorThreshold"/> to automatically
            /// destroy FSM definitions that consistently produce failing instances.
            /// Consider using <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>
            /// for thread safety.
            /// </remarks>
            private static readonly Dictionary<string, int> _fsmDefinitionErrorCounts = new();

            /// <summary>
            /// Gets or sets the maximum number of errors an FSM definition (blueprint) can accumulate across all its instances
            /// before the definition itself is automatically destroyed. Default is 3. Set to -1 to disable this feature.
            /// </summary>
            /// <remarks>
            /// This threshold helps identify and remove fundamentally broken FSM definitions that consistently
            /// produce faulty instances, preventing them from consuming excessive resources.
            /// </remarks>
            public static int DefinitionErrorThreshold { get; set; } = 3;

            /// <summary>
            /// Gets or sets the maximum number of errors an individual FSM instance can accumulate
            /// before it is automatically unregistered from the system. Default is 5. Set to -1 to disable this feature.
            /// </summary>
            /// <remarks>
            /// This threshold prevents runaway errors from a single faulty FSM instance from constantly
            /// logging errors and consuming resources, ensuring system stability.
            /// </remarks>
            public static int ErrorCountThreshold { get; set; } = 5;

            /// <summary>
            /// Gets or sets a time limit (in milliseconds) for how long an FSM processing group update can take
            /// before a warning message is logged via <see cref="OnInternalApiError"/>. Helps identify performance bottlenecks.
            /// Default is 5ms. Set to 0 or less to disable performance warnings.
            /// </summary>
            public static long TickPerformanceWarningThresholdMs { get; set; } = 5;

            /// <summary>
            /// Occurs when an internal, non-state-logic API operation throws an unexpected exception,
            /// logs a significant warning, or reports a key internal event.
            /// Provides a mechanism for users to capture internal API errors and diagnostics without forcing runtime logging.
            /// </summary>
            /// <remarks>
            /// The first parameter is a descriptive message. The second parameter is
            /// the associated <see cref="Exception"/>, if any, or <c>null</c>.
            /// </remarks>
            public static event System.Action<string, Exception>? OnInternalApiError;

            /// <summary>
            /// Safely invokes the <see cref="OnInternalApiError"/> event, checking for null subscribers.
            /// This method is the central point for triggering internal API error/warning notifications.
            /// </summary>
            /// <param name="message">The error or warning message.</param>
            /// <param name="ex">The exception that occurred, if any.</param>
            /// <remarks>
            /// As part of a single-threaded API, this method does not employ internal locks.
            /// </remarks>
            public static void InvokeInternalApiError(string message, Exception? ex)
            {
                var err = ex != null ? ex : new Exception(message);
                OnInternalApiError?.Invoke(message, err);
            }


            /// <summary>
            /// Reports an error related to a specific <see cref="FSMHandle"/> instance.
            /// Increments the instance's error count and potentially schedules its unregistration
            /// if <see cref="ErrorCountThreshold"/> is exceeded. Also contributes to the
            /// definition's cumulative error count.
            /// </summary>
            /// <param name="handle">The <see cref="FSMHandle"/> instance that encountered the error.</param>
            /// <param name="ex">The exception that occurred.</param>
            /// <remarks>
            /// This method enqueues the actual error processing to the <see cref="_deferredModifications"/> queue
            /// to ensure it happens safely at a later point in the single-threaded update cycle,
            /// preventing collection modification errors during FSM updates.
            /// This method assumes it is called from the designated single thread.
            /// </remarks>
            public static void ReportError(FSMHandle? handle, Exception ex)
            {
                if (handle == null)
                {
                    InvokeInternalApiError("Attempted to report error for a null FSMHandle.", ex);
                    return;
                }

                // 1. Handle instance-level error count
                int newInstanceCount;
                if (_errorCounts.TryGetValue(handle, out int currentInstanceCount))
                {
                    newInstanceCount = currentInstanceCount + 1;
                    _errorCounts[handle] = newInstanceCount;
                }
                else
                {
                    newInstanceCount = 1;
                    _errorCounts.Add(handle, newInstanceCount);
                }

                // Report the specific error
                InvokeInternalApiError(
                    $"FSM Instance '{handle.Name}' (Definition: '{handle.Definition.Name}') in processing group '{handle.Definition.ProcessingGroup}' reported an error. Error count: {newInstanceCount}/{ErrorCountThreshold}. Exception: {ex?.Message}",
                    ex
                );

                if (newInstanceCount >= ErrorCountThreshold)
                {
                    // Instance hit its error threshold, schedule its removal
                    FSM_API.Internal.GetDeferred().Enqueue(() =>
                    {
                        InvokeInternalApiError($"FSM Instance '{handle.Name}' (Definition: '{handle.Definition.Name}') hit Error Threshold ({ErrorCountThreshold}). Scheduling unregistration.", null);
                        FSM_API.Interaction.Unregister(handle); // This will also clear its _errorCounts entry
                        IncrementDefinitionError(handle.Definition.Name, handle.Definition.ProcessingGroup);
                    });
                }
            }

            internal static Dictionary<string, int> GetDefinitionErrorCounts()
            {
                return _fsmDefinitionErrorCounts;
            }

            internal static Dictionary<FSMHandle, int> GetErrorCounts()
            {
                return _errorCounts;
            }

            internal static void Reset(bool hardReset)
            {
                //ToDo:  Serialize the summary of any errors... or a clean bill of health.

                _errorCounts.Clear();
                _fsmDefinitionErrorCounts.Clear();
            }

            /// <summary>
            /// Internal method to increment the error count for an FSM definition.
            /// If the definition's error count exceeds <see cref="DefinitionErrorThreshold"/>,
            /// the entire definition is scheduled for destruction.
            /// </summary>
            /// <param name="fsmDefinitionName">The name of the FSM definition that had a failing instance.</param>
            /// <param name="processingGroup">The processing group of the FSM definition.</param>
            /// <remarks>
            /// This method itself schedules its work on the <see cref="_deferredModifications"/> queue,
            /// ensuring all modifications happen safely on the single designated thread.
            /// </remarks>
            private static void IncrementDefinitionError(string fsmDefinitionName, string processingGroup)
            {
                int newDefinitionCount;
                if (_fsmDefinitionErrorCounts.TryGetValue(fsmDefinitionName, out int currentDefinitionCount))
                {
                    newDefinitionCount = currentDefinitionCount + 1;
                    _fsmDefinitionErrorCounts[fsmDefinitionName] = newDefinitionCount;
                }
                else
                {
                    newDefinitionCount = 1;
                    _fsmDefinitionErrorCounts.Add(fsmDefinitionName, newDefinitionCount);
                }

                InvokeInternalApiError(
                    $"FSM Definition '{fsmDefinitionName}' in processing group '{processingGroup}' has had a failing instance removed. Definition failure count: {newDefinitionCount}/{DefinitionErrorThreshold}.",
                    null
                );

                if (newDefinitionCount >= DefinitionErrorThreshold)
                {
                    // Definition hit its error threshold, schedule its complete destruction
                    Internal.GetDeferred().Enqueue(() =>
                    {
                        InvokeInternalApiError($"FSM Definition '{fsmDefinitionName}' in processing group '{processingGroup}' hit DefinitionErrorThreshold ({DefinitionErrorThreshold}). Scheduling complete destruction.", null);
                        Interaction.DestroyFiniteStateMachine(fsmDefinitionName, processingGroup);
                    });
                }
            }
        }
    }
}