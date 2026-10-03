using System;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Integer-backed representation of a single FSM transition rule.
    /// </summary>
    /// <remarks>
    /// This is the second atomic building block of the integer-backed FSM surface.
    /// Unlike the authoring representation, transition endpoints are represented
    /// directly by integer state identities.
    /// </remarks>
    public class FSMTransition
    {
        /// <summary>
        /// The integer identity of the state this transition leaves.
        /// </summary>
        public int FromID { get; }

        /// <summary>
        /// The integer identity of the state this transition enters.
        /// </summary>
        public int ToID { get; }

        /// <summary>
        /// The condition that determines whether this transition may occur.
        /// </summary>
        public Func<IStateContext, bool> Condition { get; set; }

        /// <summary>
        /// Creates an integer-backed transition rule.
        /// </summary>
        /// <param name="fromID">The integer identity of the starting state.</param>
        /// <param name="toID">The integer identity of the destination state.</param>
        /// <param name="condition">The function that determines whether the transition may occur.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="condition"/> is null.</exception>
        public FSMTransition(int fromID, int toID, Func<IStateContext, bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "Transition condition cannot be null.");
            }

            FromID = fromID;
            ToID = toID;
            Condition = condition;
        }

        /// <summary>
        /// Evaluates the transition condition against the supplied context.
        /// </summary>
        public bool Evaluate(IStateContext context)
        {
            return Condition(context);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{FromID} --[Condition]--> {ToID}";
        }
    }
}
