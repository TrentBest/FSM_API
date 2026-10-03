using System.Collections.Generic;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Integer-keyed timer utilities for the integer-backed FSM surface.
    /// </summary>
    /// <remarks>
    /// This is the integer counterpart to the string-keyed <c>FSMTimers</c> utility. Timer identity is
    /// represented directly by an integer so callers can use manifest, protocol, or generated IDs without
    /// introducing a string lookup into their runtime path. The utility remains time-source agnostic.
    /// </remarks>
    public static class FSMTimersInt
    {
        /// <summary>Float timers keyed by integer identity.</summary>
        public static Dictionary<int, float> FloatTimers { get; } = new Dictionary<int, float>();

        /// <summary>Integer timers keyed by integer identity.</summary>
        public static Dictionary<int, int> IntTimers { get; } = new Dictionary<int, int>();

        /// <summary>The last float timer value updated.</summary>
        public static float LastFloatTime { get; private set; }

        /// <summary>The last integer timer value updated.</summary>
        public static int LastIntTime { get; private set; }

        /// <summary>Subtracts the supplied deltas from all registered timers.</summary>
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

        /// <summary>Resets an existing float timer.</summary>
        public static void ResetFloatTimer(int timerID, float setPoint = 1f)
        {
            if (FloatTimers.ContainsKey(timerID))
            {
                FloatTimers[timerID] = setPoint;
            }
        }

        /// <summary>Resets an existing integer timer.</summary>
        public static void ResetIntTimer(int timerID, int setPoint = 1)
        {
            if (IntTimers.ContainsKey(timerID))
            {
                IntTimers[timerID] = setPoint;
            }
        }

        /// <summary>Adds a float timer or replaces an existing timer with the supplied value.</summary>
        public static void AddOrSetFloatTimer(int timerID, float initialValue)
        {
            FloatTimers[timerID] = initialValue;
        }

        /// <summary>Adds an integer timer or replaces an existing timer with the supplied value.</summary>
        public static void AddOrSetIntTimer(int timerID, int initialValue)
        {
            IntTimers[timerID] = initialValue;
        }

        /// <summary>Removes a float timer.</summary>
        public static bool RemoveFloatTimer(int timerID)
        {
            return FloatTimers.Remove(timerID);
        }

        /// <summary>Removes an integer timer.</summary>
        public static bool RemoveIntTimer(int timerID)
        {
            return IntTimers.Remove(timerID);
        }
    }
}
