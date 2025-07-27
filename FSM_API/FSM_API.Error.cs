

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSingularityWorkshop.FSM_API
{
    public static partial class FSM_API
    {
        /// <summary>
        /// Provides a centralized system for managing and reporting errors and warnings within the FSM API.
        /// This class allows you to configure error thresholds for automatic cleanup of misbehaving FSMs
        /// and subscribe to internal diagnostic notifications.
        /// </summary>
        /// <remarks>
        /// The API uses a robust error handling mechanism to ensure system stability.
        /// When FSM instances or definitions encounter repeated errors (e.g., exceptions during state entry/exit,
        /// or transition evaluation), this system tracks those occurrences. If predefined thresholds are met,
        /// the problematic FSM instance or even the entire FSM definition can be automatically removed
        /// to prevent ongoing issues or resource consumption.
        /// <para>
        /// You can also subscribe to the <see cref="OnInternalApiError"/> event to receive real-time notifications
        /// about non-FSM-specific internal errors, warnings, or significant API events.
        /// </para>
        /// </remarks>
        public static class Error
        {
            private static readonly Dictionary<FSMHandle, int> _errorCounts = new();
            private static readonly Dictionary<string, int> _fsmDefinitionErrorCounts = new();

            /// <summary>
            /// The maximum number of consecutive errors an FSM instance can encounter before it is automatically shut down.
            /// </summary>
            /// <remarks>
            /// Once an FSM instance reaches this error count, it will be automatically unregistered from the system
            /// to prevent it from causing further issues.
            /// </remarks>
            public static int InstanceErrorThreshold { get; set; } = 5;

            /// <summary>
            /// The maximum number of times an FSM definition can cause an instance to be shut down due to errors
            /// before the entire definition is scheduled for destruction.
            /// </summary>
            /// <remarks>
            /// If multiple instances of the same FSM definition repeatedly fail (reaching their <see cref="InstanceErrorThreshold"/>),
            /// it might indicate a fundamental flaw in the FSM's design. When this threshold is met,
            /// the problematic FSM definition will be removed from the system, and all its remaining active instances will be shut down.
            /// </remarks>
            public static int DefinitionErrorThreshold { get; set; } = 3;

            /// <summary>
            /// Represents a delegate for internal API error events.
            /// </summary>
            /// <param name="message">A descriptive error message.</param>
            /// <param name="exception">The exception that occurred, if any.</param>
            public delegate void InternalApiErrorEventHandler(string message, Exception exception);

            /// <summary>
            /// Occurs when a non-FSM-instance-specific internal error, warning, or significant API event is reported.
            /// Subscribe to this event to receive diagnostic notifications about the FSM API's internal operations.
            /// </summary>
            public static event InternalApiErrorEventHandler OnInternalApiError;

            /// <summary>
            /// Invokes the <see cref="OnInternalApiError"/> event.
            /// This method is used internally by the FSM API to report critical non-instance-specific issues.
            /// </summary>
            /// <param name="message">A descriptive message about the error or event.</param>
            /// <param name="exception">The associated exception, or null if no exception occurred.</param>
            public static void InvokeInternalApiError(string message, Exception exception)
            {
                OnInternalApiError?.Invoke(message, exception);
            }

            /// <summary>
            /// Invokes an error specifically related to a misbehaving FSM instance.
            /// </summary>
            /// <remarks>
            /// This method tracks instance-specific errors and will automatically shut down
            /// the <paramref name="handle"/> if its error count reaches the <see cref="InstanceErrorThreshold"/>.
            /// </remarks>
            /// <param name="handle">The <see cref="FSMHandle"/> instance that encountered the error.</param>
            /// <param name="message">A descriptive message about the instance error.</param>
            /// <param name="exception">The associated exception, or null if no exception occurred.</param>
            /// <param name="processGroup">The name of the processing group where the error occurred (default is "Update").</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="handle"/> is null.</exception>

            public static void InvokeInstanceError(FSMHandle handle, string message, Exception exception, string processGroup = "Update")
            {
                if (handle == null)
                {
                    // If handle is null, it's an internal API error, not an instance error.
                    InvokeInternalApiError("InvokeInstanceError: FSMHandle provided was null.", new ArgumentNullException(nameof(handle)));
                    return;
                }

                int newCount;
                if (_errorCounts.TryGetValue(handle, out int currentCount))
                {
                    newCount = currentCount + 1;
                    _errorCounts[handle] = newCount;
                }
                else
                {
                    newCount = 1;
                    _errorCounts.Add(handle, newCount);
                }
               
                InvokeInternalApiError(
                    $"FSM Instance '{handle.Name}' (Context ID: {handle.Context.GetHashCode()}) in group '{processGroup}' encountered error in state '{handle.CurrentState}'. Count: {newCount}/{InstanceErrorThreshold}. Message: {message}",
                    exception
                );

                if (newCount >= InstanceErrorThreshold)
                {
                    // Defer shutdown to avoid re-entrancy issues during error handling or updates
                    Internal.GetDeferred().Enqueue(() =>
                    {
                        InvokeInternalApiError(
                            $"FSM Instance '{handle.Name}' (Context ID: {handle.Context.GetHashCode()}) in group '{processGroup}' hit InstanceErrorThreshold ({InstanceErrorThreshold}). Shutting down.",
                            null
                        );
                        // Record a definition error before shutting down the instance
                        InvokeDefinitionError(handle.Definition.Name, processGroup);
                        handle.ShutDown(); // This will unregister the handle
                        _errorCounts.Remove(handle); // Clean up error count for this handle
                    });
                }
            }

            /// <summary>
            /// Invokes an error specifically related to an FSM definition, typically when an instance
            /// derived from it has been shut down due to repeated errors.
            /// </summary>
            /// <remarks>
            /// This method tracks definition-specific errors and will automatically schedule the
            /// complete destruction of the FSM definition and all its instances if its error count
            /// reaches the <see cref="DefinitionErrorThreshold"/>.
            /// </remarks>
            /// <param name="fsmDefinitionName">The name of the FSM definition that caused the error.</param>
            /// <param name="processingGroup">The processing group of the FSM definition.</param>
            /// <exception cref="ArgumentException">Thrown if <paramref name="fsmDefinitionName"/>
            /// or <paramref name="processingGroup"/> is null, empty, or whitespace.</exception>
            public static void InvokeDefinitionError(string fsmDefinitionName, string processingGroup)
            {
                if (string.IsNullOrWhiteSpace(fsmDefinitionName))
                {
                    InvokeInternalApiError("InvokeDefinitionError: FSM Definition Name cannot be null or empty.", new ArgumentException(nameof(fsmDefinitionName)));
                    return;
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    InvokeInternalApiError("InvokeDefinitionError: Processing Group cannot be null or empty.", new ArgumentException(nameof(processingGroup)));
                    return;
                }

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
                    $"FSM Definition '{fsmDefinitionName}' in processing group '{processingGroup}' has had a failing instance removed. Definition failure count: {newDefinitionCount}/{DefinitionErrorThreshold}. (To adjust this threshold, modify FSM_API.Error.DefinitionErrorThreshold.)",
                    null
                );

                if (newDefinitionCount >= DefinitionErrorThreshold)
                {
                    Internal.GetDeferred().Enqueue(() =>
                    {
                        InvokeInternalApiError($"FSM Definition '{fsmDefinitionName}' in processing group '{processingGroup}' hit DefinitionErrorThreshold ({DefinitionErrorThreshold}). Scheduling complete destruction.", null);
                        Interaction.DestroyFiniteStateMachine(fsmDefinitionName, processingGroup);
                    });
                }
            }

            /// <summary>
            /// Resets the accumulated error count for a specific FSM definition.
            /// This is typically called when an FSM definition is explicitly destroyed
            /// or reloaded, to clear its error history.
            /// </summary>
            /// <param name="fsmDefinitionName">The name of the FSM definition whose error count to reset.</param>
            internal static void ResetDefinitionErrorCount(string fsmDefinitionName)
            {
                // Note: No processingGroup needed here as _fsmDefinitionErrorCounts key is just fsmDefinitionName.
                // If it were keyed by (fsmName, processingGroup), then processingGroup would be needed.
                _fsmDefinitionErrorCounts.Remove(fsmDefinitionName);
            }

            internal static Dictionary<FSMHandle, int> GetErrorCounts()
            {
                return _errorCounts;
            }

            internal static Dictionary<string, int> GetDefinitionErrorCounts()
            {
                return _fsmDefinitionErrorCounts;
            }

            internal static void Reset()
            {
                _fsmDefinitionErrorCounts.Clear();
                _errorCounts.Clear();
            }
        }
    }
}