using System.Collections.Generic;
using System.Linq;




namespace TheSingularityWorkshop.FSM_API
{
public static partial class FSM_API
    {
public static partial class Interaction
        {
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