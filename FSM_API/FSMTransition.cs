using System;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// This class represents a single **"transition rule"** in your FSM (Finite State Machine) blueprint.
    /// Think of it as a specific pathway 🛤️ that tells your FSM how to move from one "state"
    /// (like "Idle") to another "state" (like "Running").
    /// </summary>
    /// <remarks>
    /// Each transition rule has three main parts:
    /// <list type="bullet">
    ///     <item><term>From:</term><description>The starting state.</description></item>
    ///     <item><term>To:</term><description>The ending state.</description></item>
    ///     <item><term>Condition:</term><description>A special check that must be true for the transition to happen.</description></item>
    /// </list>
    /// <para>
    /// While you'll often create these transitions easily using the FSMBuilder,
    /// this class is made public for more advanced users who might need to
    /// load FSM definitions from files or perform very specific tweaks.
    /// </para>
    /// </remarks>
    public class FSMTransition
    {
        /// <summary>
        /// This is the **name of the state** where this transition starts.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Refactored to hash V1.0.14
        /// </remarks>
        public int FromID { get; private set; }

        /// <summary>
        /// This is the **name of the state** where this transition leads to.
        /// </summary>
        public string To { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Refactored to hash V1.0.14
        /// </remarks>
        public int ToID { get; private set; }


        /// <summary>
        /// This is the **condition** (a function that returns `true` or `false`)
        /// that must be true for this transition to actually happen.
        /// </summary>
        /// <remarks>
        /// This function receives an <see cref="IStateContext"/>, which provides
        /// important information about the FSM's current situation,
        /// helping the condition decide if the transition should occur.
        /// </remarks>
        public Func<IStateContext, bool> Condition { get; set; }

        /// <summary>
        /// Creates a new transition rule for your FSM blueprint.
        /// </summary>
        /// <remarks>
        /// When you create a transition, you must tell it:
        /// <list type="bullet">
        ///     <item><term>Where it starts (`from`):</term><description>The state you're leaving.</description></item>
        ///     <item><term>Where it goes (`to`):</term><description>The state you're entering.</description></item>
        ///     <item><term>What makes it happen (`condition`):</term><description>The rule that must be true.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="from">The name of the state this transition starts from.</param>
        /// <param name="to">The name of the state this transition moves to.</param>
        /// <param name="condition">The function that decides if this transition should happen.</param>
        /// <exception cref="ArgumentException">
        /// This happens if the `from` or `to` state names are empty or just spaces.
        /// It means you haven't given valid names for the states.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// This happens if you don't provide a `condition` (it's `null`).
        /// A transition always needs a rule to decide when to move.
        /// </exception>
        public FSMTransition(string from, string to, Func<IStateContext, bool> condition)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new ArgumentException("Transition 'from' state name cannot be null or empty.", nameof(@from));
            }
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Transition 'to' state name cannot be null or empty.", nameof(to));
            }
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "Transition condition cannot be null.");
            }

            From = from;
            To = to;
            Condition = condition;
        }

        /// <summary>
        /// Gives a simple text description of this transition rule.
        /// </summary>
        /// <returns>
        /// A string that looks like "StartingState --[Condition]--> EndingState".
        /// It helps you quickly see what the transition does.
        /// </returns>
        public override string ToString()
        {
            // We can't easily print the condition's internal details for Func<T, bool>,
            // but we can indicate its presence.
            return $"{From} --[Condition]--> {To}";
        }
    }
}