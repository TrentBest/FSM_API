using System;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// This is your direct link, or **"handle,"** to a specific, live FSM (Finite State Machine) instance. 🎮
    /// Think of it as the remote control for one particular character or object that's using an FSM.
    /// <div class="mermaid">
    /// classDiagram
    ///    direction LR
    ///    class FSMHandle {
    ///        +string Name
    ///        +string CurrentState
    ///        +IStateContext Context
    ///    }
    ///    class FSM {
    ///        +string Name
    ///        +string InitialState
    ///    }
    ///    class IStateContext {
    ///        &lt;&lt;interface&gt;&gt;
    ///        +string Name
    ///        +bool IsValid
    ///    }
    ///
    ///    FSMHandle "1" *-- "1" FSM : &lt;&lt;uses&gt;&gt;
    ///    FSMHandle "1" *-- "1" IStateContext : &lt;&lt;controls&gt;&gt;
    /// </div>
    /// <div class="mermaid">
    /// graph TD
    ///    A[FSMHandle (The Logic)] -->|Commands| B[IStateContext (The Data)];
    ///    B -->|Returns Data/Status| A;
    /// </div>
    /// <div class="mermaid">
    /// sequenceDiagram
    ///    participant H as FSMHandle
    ///    participant D as FSMDefinition
    ///    participant S as CurrentState
    ///    participant C as Context
    ///
    ///    H->>D: Step(CurrentState, Context)
    ///    D->>S: Update(Context)
    ///    note right of S: Performs state logic
    ///    D->>D: Check Any-State Transitions
    ///    D->>D: Check Regular Transitions
    ///    alt If a transition condition is met
    ///        D->>S: Exit(Context)
    ///        D->>+D: Find New State
    ///        D->>-H: Return NewStateName
    ///        H->>H: CurrentState = NewStateName
    ///        H->>D: ForceTransition(OldState, NewState, Context)
    ///        D->>+S: Exit(Context)
    ///        S->>+S: Enter(Context)
    ///    else No transition
    ///        D->>H: Return CurrentStateName
    ///    end
    /// </div>
    /// !(https://raw.githubusercontent.com/trentbest/fsm_api/FSM_API-72aeabda0e28b301ddd2e675e1f49da584cf105c/Documentation/User%20Guide/Visuals/State_Lifecycle.png)
    /// </summary>
    /// <remarks>
    /// This handle is how you, as a game designer or system integrator, will typically interact with an FSM
    /// that's currently running. It allows you to:
    /// <list type="bullet">
    ///     <item>Find out **what state** the FSM is currently in (<see cref="CurrentState"/>).</item>
    ///     <item>Tell the FSM to **change states immediately** (<see cref="TransitionTo(string)"/>).</item>
    ///     <item>And generally **manage** its life (like starting or stopping it).</item>
    /// </list>
    /// <para>
    /// It also gives you access to the FSM's <see cref="IStateContext"/>, which holds all the unique data
    /// for *this specific FSM instance*.
    /// </para>
    ///
    /// **Tips for Using Your FSM Handle:**
    /// <list type="bullet">
    ///     <item>
    ///         ✅ **Safest Way:** Mostly, you'll just want to **check the FSM's <see cref="CurrentState"/>**.
    ///         For example: `if (myCharacter.CurrentState == "Idle") { /* do something */ }`.
    ///         This lets the FSM handle its own internal changes, which is generally best.
    ///     </item>
    ///     <item>
    ///         ⚠️ **Advanced Use (Use with Care!):** You *can* directly access and change the
    ///         <see cref="Context"/> (e.g., `myCharacter.Context.Health = 10;`). This is powerful,
    ///         but it means you're stepping outside the FSM's own defined rules, so be careful not to
    ///         break its intended behavior! Usually, context changes happen *inside* your FSM's
    ///         state actions (<c>OnEnter</c>, <c>OnUpdate</c>, <c>OnExit</c>).
    ///     </item>
    /// </list>
    /// </remarks>
    public class FSMHandle
    {
        /// <summary>
        /// Handle's Id
        /// </summary>
        public int Id { get; internal set; } = -1;
        /// <summary>
        /// This is the currently assigned **blueprint (definition)** of the FSM that this handle is controlling.
        /// </summary>
        /// <remarks>
        /// It's like having the architectural plans for a house. You can look at them to understand
        /// the house's design (its states and transitions), but you can't change the house by
        /// drawing on these plans here. This is set when the FSM instance is first created.
        /// </remarks>

        public FSM Definition { get; internal set; }

        /// <summary>
        /// This is the **data bag** 🎒 (context) specific to *this particular* FSM instance.
        /// </summary>
        /// <remarks>
        /// It holds the changing information that your FSM's actions read or write. The context is
        /// supplied and owned by the caller; FSM_API does not dispose it or use its validity flag to
        /// represent this handle's registration state. Multiple handles may reference the same context.
        /// <para>
        /// You *can* directly change this context data here (e.g., `handle.Context.Score = 100;`).
        /// However, it's generally safer and clearer if these changes happen through the
        /// FSM's own `OnEnter`, `OnUpdate`, or `OnExit` actions for each state.
        /// Changing it directly from outside might bypass your FSM's carefully planned logic!
        /// </para>
        /// </remarks>
        public IStateContext Context { get; set; } = null;

        /// <summary>
        /// The processing group currently responsible for scheduling this FSM instance.
        /// This instance-owned scheduling metadata may change when the instance moves between compatible cohorts.
        /// </summary>
        public string ProcessingGroup { get; internal set; } = "Update";

        /// <summary>
        /// This tells you the **current state name** of this FSM instance.
        /// </summary>
        /// <remarks>
        /// For example, if your FSM is for a character, this might tell you "Idle," "Walking," or "Jumping."
        /// This is the **best and safest way** to check what your FSM is doing.
        /// This state name is updated automatically when the FSM moves between states.
        /// </remarks>
        public string CurrentState { get; internal set; } = "UnknownState";

        /// <summary>
        /// This is the **name of the FSM blueprint** that this handle is using.
        /// </summary>
        /// <remarks>
        /// It's a quick way to get the name from the <see cref="Definition"/> blueprint itself.
        /// </remarks>
        public string Name => Definition.Name;

        /// <summary>
        /// Reports whether the assigned context currently permits this handle to be processed.
        /// </summary>
        /// <remarks>
        /// This property mirrors <see cref="IStateContext.IsValid"/>; it is not an independent
        /// lifecycle flag. When false, the scheduler skips this handle but leaves it registered.
        /// Restoring a reusable context's validity allows the same handle to resume. Use
        /// <see cref="FSM_API.Interaction.DestroyInstance(FSMHandle)"/> when the instance should
        /// be removed and torn down instead.
        /// </remarks>
        public bool IsValid => Context?.IsValid ?? false;

        /// <summary>
        /// Gets whether this handle has been explicitly torn down and removed from API management.
        /// This lifecycle flag is independent of the caller-owned context's <see cref="IsValid"/> value.
        /// </summary>
        public bool IsDestroyed { get; private set; } = false;

        /// <summary>
        /// Tracks whether the current state's entry action has run for this handle.
        /// This is distinct from <see cref="IsValid"/>: validity controls scheduler eligibility,
        /// while this flag prevents a resumed or repeatedly updated handle from being mistaken
        /// for an instance that has never entered its current state.
        /// </summary>
        public bool HasEnteredCurrentState { get; internal set; } = false;


        /// <summary>
        /// Sets up a new FSM handle, linking it to an FSM blueprint and its specific data.
        /// </summary>
        /// <remarks>
        /// When you create an FSM handle:
        /// <list type="bullet">
        ///     <item>It needs an `FSM` <paramref name="definition"/> (the blueprint).</item>
        ///     <item>It needs a `context` (the data bag) specific to this running instance.</item>
        /// </list>
        /// Upon creation, the handle's <see cref="CurrentState"/> property is immediately set to the FSM's <see cref="FSM.InitialState"/>.
        /// However, the `OnEnter` action for this state will not be executed until the FSM instance is first stepped or updated, typically
        /// by the API's internal tick system. This prevents unexpected behavior from 'OnEnter' actions running during the handle's instantiation.
        /// </remarks>
        /// <param name="definition">The blueprint (<see cref="FSM"/>) for this FSM instance.</param>
        /// <param name="context">The data bag (<see cref="IStateContext"/>) for this specific FSM instance.</param>
        /// <param name="id">ID of the handle</param>
        /// <exception cref="ArgumentNullException">
        /// This happens if you try to create an FSM handle without a blueprint (`definition` is `null`)
        /// or without a data bag (`context` is `null`).
        /// </exception>
        public FSMHandle(FSM definition = null, IStateContext context = null, int id = -1, string processingGroup = "Update")
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition), "FSM definition cannot be null for FSMHandle.");
            Context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null for FSMHandle.");
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }
            Id = id;
            ProcessingGroup = processingGroup;
            CurrentState = Definition.InitialState;

            // A successfully constructed handle starts eligible for its next scheduled update.
            // The context remains caller-owned; this initializes its execution gate without
            // coupling validity to registration or destruction.
            Context.IsValid = true;
        }

        /// <summary>
        /// Tells this FSM instance to take one **"step"** or "tick" forward. 🏃
        /// </summary>
        /// <remarks>
        /// During this step:
        /// <list type="bullet">
        ///     <item>If the current state has not been entered, its `OnEnter` action runs first.</item>
        ///     <item>The current state's `OnUpdate` action runs.</item>
        ///     <item>Any-State transitions are evaluated before regular state-specific transitions.</item>
        ///     <item>When a transition matches, the current state's `OnExit` action runs and
        ///     <see cref="CurrentState"/> changes to the target. The target state's `OnEnter`
        ///     action runs at the start of the next update.</item>
        /// </list>
        /// Explicit forced transitions such as <see cref="TransitionTo(string)"/> enter their target
        /// immediately rather than waiting for the next update.
        /// <para>
        /// **IMPORTANT: Usually, you won't call this directly!** This method is typically called
        /// automatically by the FSM_API's internal system on a regular schedule (like every frame in a game).
        /// Only call this manually if you're building a very specific or custom update system for your FSMs.
        /// Calling it incorrectly can lead to unexpected behavior.
        /// </para>
        /// </remarks>
        /// <param name="processGroup">
        /// An optional name for the group this FSM belongs to, mainly used for error reporting.
        /// It defaults to "Update".
        /// </param>
        public void Update(string processGroup = "Update")
        {
            // Manual stepping must honor IsValid just like scheduled ticks. A caller-owned
            // context may be temporarily unavailable; that suspends computation, not lifecycle.
            if (IsDestroyed || Context == null || !Context.IsValid)
            {
                return;
            }

            try
            {
                // Manual updates must honor the same first-entry lifecycle as scheduled ticks.
                // A transition marks the target as not yet entered; the next update will enter it.
                if (!HasEnteredCurrentState)
                {
                    var currentState = Definition.GetState(CurrentState);
                    if (currentState != null)
                    {
                        currentState.Enter(Context);
                        HasEnteredCurrentState = true;
                    }
                }

                // OnEnter may suspend the caller-owned context. Do not run OnUpdate
                // or transition guards after that callback closes the execution gate.
                if (!Context.IsValid)
                {
                    return;
                }

                string nextState;
                Definition.Step(CurrentState, Context, out nextState, this);
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
        /// **Forces** this FSM instance to immediately change to a specific state,
        /// ignoring any normal transition rules. 🛑
        /// </summary>
        /// <remarks>
        /// Use this when you need absolute control, like manually setting a character's
        /// state to "Dead" regardless of what they were doing.
        /// <para>
        /// When you force a transition, the FSM will:
        /// <list type="bullet">
        ///     <item>Run the `OnExit` action of its current state.</item>
        ///     <item>Immediately run the `OnEnter` action of the <paramref name="nextStateName"/>.</item>
        /// </list>
        /// </para>
        /// **Caution:** Bypassing normal FSM logic can sometimes lead to unexpected results
        /// if the target state isn't ready for a sudden jump.
        /// </remarks>
        /// <param name="nextStateName">The name of the state you want the FSM to jump to.</param>
        /// <exception cref="ArgumentException">
        /// This happens if the `nextStateName` is empty, or if that state doesn't exist
        /// in your FSM's blueprint.
        /// </exception>
        public void TransitionTo(string nextStateName)
        {
            if (string.IsNullOrWhiteSpace(nextStateName))
            {
                throw new ArgumentException("Target state name cannot be null or empty.", nameof(nextStateName));
            }

            try
            {
                Definition.ForceTransition(CurrentState, nextStateName, Context);
                CurrentState = nextStateName;
                HasEnteredCurrentState = Definition.HasState(nextStateName);
            }
            catch (Exception ex)
            {
                // Report any issues during a forced transition as a "fubar".
                FSM_API.Error.InvokeInstanceError(this, $"Transition to:  {nextStateName} failed.", ex);
                throw; // Re-throw the exception as this is a direct user-invoked method.
            }
        }



        /// <summary>
        /// Resets this FSM instance back to its **very first state** (the <see cref="FSM.InitialState"/>)
        /// as defined in its blueprint. ↩️
        /// </summary>
        /// <remarks>
        /// This is like pressing a "restart" button for this specific FSM. It will:
        /// <list type="bullet">
        ///     <item>Run the `OnExit` action of its current state.</item>
        ///     <item>Then run the `OnEnter` action of its initial state.</item>
        /// </list>
        /// This is very useful for getting a complex FSM back to a known starting point.
        /// </remarks>
        public void ResetFSMInstance()
        {
            Definition.ForceTransition(CurrentState, Definition.InitialState, Context);
            CurrentState = Definition.InitialState;
            HasEnteredCurrentState = Definition.HasState(CurrentState);
        }


        /// <summary>
        /// Performs instance teardown after the API removes this handle from its registered collection. 🚫
        /// </summary>
        /// <remarks>
        /// This executes the current state's exit callback when the state exists. The API removes
        /// the handle from its registration before calling this method. Teardown must not invalidate,
        /// dispose, or otherwise take ownership of <see cref="Context"/> because the caller owns it
        /// and other handles may share it.
        /// </remarks>
        internal void DestroyHandle()
        {
            if (IsDestroyed)
            {
                return;
            }

            // Mark the handle first so re-entrant callbacks and a scheduler snapshot cannot
            // execute it again after explicit teardown begins.
            IsDestroyed = true;

            if (Definition != null && !string.IsNullOrEmpty(CurrentState))
            {
                // Try to get the current state object.
                var currentStateObject = Definition.GetState(CurrentState);

                // If the state object is found, call its Exit method.
                // This handles cases where an instance's state might be
                // a stale string name for a state that no longer exists in the definition.
                if (currentStateObject != null)
                {
                    currentStateObject.Exit(Context);
                }
            }

            // The context is supplied and owned by the caller. Teardown removes this
            // FSM_API-owned handle/registration; it must not mutate the shared context's
            // validity because other handles may still use the same context.

        }

        /// <summary>
        /// Manually checks all the rules (conditions) for leaving the FSM's <see cref="CurrentState"/>.
        /// </summary>
        /// <remarks>
        /// This method is particularly useful if your FSM is set up to *not* automatically update
        /// (meaning its `ProcessRate` in the FSM blueprint is set to `0`). In such cases,
        /// you would call `EvaluateConditions()` yourself whenever something happens that
        /// might cause a state change (like a player pressing a button, or an enemy seeing you).
        /// <para>
        /// If a transition's condition is met, the FSM will move to that new state.
        /// Only the *first* transition whose condition is true will be taken in a single evaluation.
        /// </para>
        /// </remarks>
        public void EvaluateConditions()
        {
            // Manual evaluation follows the same precedence as scheduled stepping:
            // Any-State transitions first, then transitions originating in CurrentState.
            if (IsDestroyed || Context == null || !Context.IsValid)
            {
                return;
            }

            var possibleTransitions = Definition.GetAllTransitions()
                .Where(t => t.From == FSM.AnyStateIdentifier || t.From == "*" || t.From == CurrentState)
                .ToList();

            foreach (var transition in possibleTransitions)
            {
                // Do not invoke user guards for invalid destinations.
                if (!Definition.HasState(transition.To))
                {
                    continue;
                }

                bool matches;
                try
                {
                    matches = transition.Condition != null && transition.Condition.Invoke(Context);
                }
                catch (Exception ex)
                {
                    // A guard failure is isolated to that candidate; later transitions may still match.
                    FSM_API.Error.InvokeInternalApiError(
                        $"Error evaluating manual transition condition from '{CurrentState}' to '{transition.To}' in FSM '{Name}'. Exception: {ex.Message}",
                        ex);
                    continue;
                }

                if (!matches)
                {
                    continue;
                }

                // Guards can mutate the runtime-editable definition.
                if (!Definition.HasState(transition.To))
                {
                    continue;
                }

                try
                {
                    Definition.ForceTransition(CurrentState, transition.To, Context);
                }
                catch
                {
                    // ForceTransition can run Exit before an Enter callback throws. Do not
                    // pretend the old state is still fully entered, and never try another
                    // candidate after a partially completed transition.
                    HasEnteredCurrentState = false;
                    throw;
                }

                // ForceTransition's Exit callback can remove the target; it logs and returns
                // without entering it in that case. Keep the prior state identity but mark
                // its lifecycle as requiring re-entry.
                if (!Definition.HasState(transition.To))
                {
                    HasEnteredCurrentState = false;
                    return;
                }

                CurrentState = transition.To;
                HasEnteredCurrentState = true;
                return; // One transition per evaluation; Any-State candidates are ordered first.
            }
        }
    }
}