using System;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Represents a live, lightweight accessor to a running Finite State Machine (FSM) instance.
    /// <para>
    /// This handle is the primary interface for external systems to interact with a specific FSM instance.
    /// It allows you to query the FSM's <see cref="CurrentState"/>, trigger manual transitions,
    /// and manage its lifecycle. It also provides direct access to the underlying <see cref="IStateContext"/>
    /// for scenarios where explicit manipulation by a controlling script is necessary.
    /// </para>
    /// </summary>
    /// <remarks>
    /// **Preferred Usage Pattern (Lower Risk):**
    /// For most FSM interactions, it is highly recommended to primarily query the FSM's behavior
    /// by checking its <see cref="CurrentState"/> (e.g., <c>if (handle.CurrentState == "Init") { /* ... */ }</c>).
    /// This approach maintains the FSM's internal control over state transitions and context modifications.
    ///
    /// **Direct Context Access (Higher Risk / Advanced Use):**
    /// While direct access to the <see cref="Context"/> property is provided (e.g., <c>handle.Context.SomeProperty = value;</c>),
    /// this is intended for specific "controlling script" scenarios where direct manipulation or
    /// even reassignment of the context object is required. Be aware that modifying the context
    /// directly outside of the FSM's defined state actions (<c>OnEnter</c>, <c>OnUpdate</c>, <c>OnExit</c>)
    /// can bypass intended FSM logic and potentially lead to unexpected behavior if not managed carefully.
    /// </remarks>
    public class FSMHandle
    {
        /// <summary>
        /// Gets the underlying Finite State Machine definition this handle refers to.
        /// This provides access to the FSM's immutable structure (states, transitions).
        /// </summary>
        /// <remarks>
        /// This is a read-only field, set once at the time of instance creation.
        /// While providing access to the FSM's definition, typical interaction should
        /// be via this <see cref="FSMHandle"/> instance.
        /// </remarks>

        public readonly FSM Definition;

        /// <summary>
        /// Gets or sets the context object associated with this FSM instance.
        /// The context object holds the application-specific data and methods that the FSM operates on.
        /// It must implement the <see cref="IStateContext"/> interface.
        /// </summary>
        /// <remarks>
        /// This property allows direct access to, and even reassignment of, the <see cref="IStateContext"/>
        /// instance. While powerful for "controlling scripts" that need to explicitly manage the context,
        /// it is generally recommended that modifications to the context's *data* are performed by
        /// the FSM's state actions (<c>OnEnter</c>, <c>OnUpdate</c>, <c>OnExit</c> methods within your
        /// <see cref="FSMState"/> definitions). Direct external manipulation of the context
        /// can bypass the FSM's intended flow and should be used with caution and a clear understanding
        /// of its implications on the FSM's integrity.
        /// </remarks>

        public IStateContext Context { get; set; }

        /// <summary>
        /// Gets the current state of this FSM instance.
        /// <para>
        /// **This is the preferred and safest way to query the FSM's current behavior.**
        /// It allows you to react to the FSM's state without directly manipulating its context.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This property is updated internally by the FSM's <see cref="Update"/> and <see cref="TransitionTo"/> methods.
        /// </remarks>
        public string CurrentState { get; private set; }

        /// <summary>
        /// Gets the name of the FSM definition that this handle is an instance of.
        /// This is a convenience property that retrieves the name directly from the underlying FSM definition.
        /// </summary>
        public string Name => Definition.Name;

        /// <summary>
        /// Gets a value indicating whether this FSMHandle is currently valid and active.
        /// Validity is determined by whether its <see cref="Context"/> is not null and its
        /// <see cref="IStateContext.IsValid"/> property returns <c>true</c>.
        /// </summary>
        public bool IsValid => Context?.IsValid ?? false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FSMHandle"/> class.
        /// </summary>
        /// <param name="definition">The <see cref="FSM"/> definition this handle will represent.</param>
        /// <param name="context">The <see cref="IStateContext"/> object that this FSM instance will operate on.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="definition"/> or <paramref name="context"/> is null.</exception>
        public FSMHandle(FSM definition, IStateContext context)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition), "FSM definition cannot be null for FSMHandle.");
            Context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null for FSMHandle.");

            CurrentState = Definition.InitialState;
            Definition.EnterInitial(Context);
        }

        /// <summary>
        /// Advances the FSM instance by one logical step. This involves executing the 'OnUpdate'
        /// action of the <see cref="CurrentState"/> and evaluating all defined transitions
        /// from the <see cref="CurrentState"/>. If a valid transition is found, the FSM will
        /// move to the next state, executing 'OnExit' for the old state and 'OnEnter' for the new state.
        /// </summary>
        /// <param name="processGroup">Optional: A string identifying the process group for error reporting, defaults to "Update".</param>
        /// <remarks>
        /// **CAUTION:** This method is primarily intended to be called by the FSM_API's internal
        /// tick loop or an equivalent periodic update mechanism within your application.
        /// Directly calling <c>Update()</c> manually should only be done by those who
        /// have a deep understanding of the FSM's internal processing and lifecycle.
        /// Incorrect manual invocation can lead to unexpected state behavior or performance issues.
        /// Use with extreme care and only when building a highly customized update scheduler.
        /// </remarks>
        public void Update(string processGroup = "Update")
        {
            try
            {
                // The Step method will internally handle state transitions and update CurrentState
                Definition.Step(CurrentState, Context, out string nextState);
                CurrentState = nextState;
            }
            catch (Exception ex)
            {
                // If an exception occurs during the FSM's internal step, report it as a "fubar".
                // This allows the FSM_API to track and potentially remove problematic instances.
                FSM_API.Error.InvokeInstanceError(this, $"FSMHandle:  {Name} has crashed:  {ex.Message}", ex, processGroup);
            }
        }

        /// <summary>
        /// Forces an immediate transition of the FSM to a specified state,
        /// bypassing normal transition conditions.
        /// </summary>
        /// <param name="nextStateName">The name of the state to transition to.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="nextStateName"/> is null or empty,
        /// or if the target state does not exist within the FSM definition.</exception>
        /// <remarks>
        /// Use this method with caution, as forcing a transition may bypass intended FSM logic
        /// and potentially put the FSM into an unexpected state if conditions are not met.
        /// This method will execute the 'Exit' action of the current state and
        /// the 'Enter' action of the target state.
        /// </remarks>
        public void TransitionTo(string nextStateName)
        {
            if (string.IsNullOrWhiteSpace(nextStateName))
            {
                throw new ArgumentException("Target state name cannot be null or empty.", nameof(nextStateName));
            }

            try
            {
                Definition.ForceTransition(CurrentState, nextStateName, Context);
                //CurrentState = nextStateName;
            }
            catch (Exception ex)
            {
                // Report any issues during a forced transition as a "fubar".
                FSM_API.Error.InvokeInstanceError(this, $"Transition to:  {nextStateName} failed.", ex);
                throw; // Re-throw the exception as this is a direct user-invoked method.
            }
        }



        /// <summary>
        /// Resets the FSM instance to its initial state as defined by its <see cref="FSM.InitialState"/>.
        /// </summary>
        /// <remarks>
        /// This method forces a transition from the current state to the FSM's initial state,
        /// triggering the 'Exit' action of the current state and the 'Enter' action of the initial state.
        /// It effectively restarts the FSM's state machine logic, useful for resetting complex behaviors.
        /// </remarks>
        public void ResetFSMInstance()
        {
            Definition.ForceTransition(CurrentState, Definition.InitialState, Context);
        }


        /// <summary>
        /// Shuts down and unregisters this FSM instance from the API's internal management system.
        /// </summary>
        /// <remarks>
        /// This method is crucial for proper resource management, especially in long-running applications
        /// or scenarios with many FSM instances. It ensures that the FSM is no longer processed by any
        /// internal update loops and is removed from memory. After calling <c>ShutDown()</c>, this
        /// <see cref="FSMHandle"/> instance should be considered invalid and should not be used for
        /// further operations. It delegates the actual unregistration and cleanup to
        /// <see cref="FSM_API.Interaction.Unregister(FSMHandle)"/>.
        /// </remarks>
        internal void ShutDown()
        {
            FSM_API.Interaction.Unregister(this);
        }

        /// <summary>
        /// Explicitly evaluates the outgoing transition conditions from the FSM instance's current state.
        /// </summary>
        /// <remarks>
        /// This method is intended for manual or event-driven FSM progression,
        /// especially when the FSM's <see cref="FSMModifier.WithProcessRate"/> is set to <c>0</c> (event-driven).
        /// <para>
        /// It iterates through all transitions originating from the <see cref="CurrentState"/> and
        /// attempts to execute the first transition whose condition evaluates to <c>true</c>.
        /// If a transition occurs, the FSM instance's <see cref="CurrentState"/> will change,
        /// and the corresponding <c>OnExit</c> and <c>OnEnter</c> actions will be invoked.
        /// </para>
        /// </remarks>
        public void EvaluateConditions()
        {
            // Get all potential transitions originating from the current state.
            // Convert to a list to avoid issues if a transition itself causes modifications
            // to the underlying Definition's transition collection (though unlikely here).
            var possibleTransitions = Definition.GetAllTransitions()
                                                .Where(t => t.From == CurrentState)
                                                .ToList();

            // Iterate through possible transitions to find the first one whose condition is met.
            // It's generally best practice to only take one transition per evaluation cycle
            // to ensure predictable FSM behavior and avoid race conditions or unexpected state jumps
            // if multiple conditions are simultaneously true.
            foreach (var transition in possibleTransitions)
            {
                // Check if a condition is defined and if it evaluates to true for the current context.
                if (transition.Condition != null && transition.Condition.Invoke(Context))
                {
                    // Found a valid transition. Force the transition and then exit,
                    // as only one transition should typically occur per evaluation.
                    Definition.ForceTransition(CurrentState, transition.To, Context);
                    return; // Exit after the first successful transition
                }
            }
        }
    }
}