using System;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Runtime handle for an integer-backed FSM instance.
    /// </summary>
    /// <remarks>
    /// The handle deliberately stores only integer FSM/state identities and the instance context.
    /// The successful execution path performs no string state lookup and exposes no string process-group identity.
    /// <para>
    /// <see cref="FSMHandleInt"/> is the explicit public handle type returned by the integer runtime.
    /// This base remains available so the representation-specific runtime can share one implementation.
    /// </para>
    /// </remarks>
    public class FSMHandle
    {
        private bool _isExecutingExitCallback;
        /// <summary>Unique identity of this running FSM instance.</summary>
        public int Id { get; internal set; }

        /// <summary>The integer-backed FSM blueprint controlled by this handle.</summary>
        public readonly FSM Definition;

        /// <summary>The blueprint identity.</summary>
        public int FSM_ID => Definition.FSM_ID;

        /// <summary>The instance-specific state context.</summary>
        public IStateContext Context { get; set; }

        /// <summary>The integer identity of the current state.</summary>
        public int CurrentStateID { get; internal set; }

        /// <summary>Indicates whether the instance context is valid.</summary>
        public bool IsValid => Context?.IsValid ?? false;

        /// <summary>Indicates whether the current state has been entered.</summary>
        public bool HasEnteredCurrentState { get; internal set; }

        /// <summary>Indicates whether this handle has completed teardown.</summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>Indicates that runtime teardown has been scheduled.</summary>
        internal bool IsPendingDestruction { get; set; }

        /// <summary>Creates an integer-backed FSM handle.</summary>
        public FSMHandle(FSM definition, IStateContext context, int id = -1)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Id = id;
            CurrentStateID = Definition.InitialStateID;

            // Match the string-backed constructor contract: a successfully created handle
            // begins eligible for execution. The context remains caller-owned.
            Context.IsValid = true;
        }

        /// <summary>
        /// Enters the current initial state before the first update.
        /// </summary>
        /// <remarks>
        /// Initialization is explicit and idempotent. The successful lifecycle path contains no string identity.
        /// </remarks>
        public void Initialize()
        {
            if (IsDestroyed || IsPendingDestruction || HasEnteredCurrentState || Context == null || !Context.IsValid)
            {
                return;
            }

            // Capture the identity being entered before invoking user code. The
            // Enter callback may mutate the definition (including removing the
            // initial state and causing InitialStateID to select another state).
            // Keep this handle on the state whose Enter callback actually ran.
            var initialStateID = Definition.InitialStateID;
            Definition.EnterInitial(Context);

            // The Enter callback can re-enter the runtime and destroy this handle
            // (for example, by unregistering its definition). Teardown owns the
            // final lifecycle state; initialization must not revive the handle.
            if (IsDestroyed || IsPendingDestruction)
            {
                return;
            }

            CurrentStateID = initialStateID;
            HasEnteredCurrentState = Definition.HasState(initialStateID);
        }

        /// <summary>Advances this FSM instance by one step.</summary>
        /// <remarks>
        /// Normal execution uses integer identities throughout the path from the handle to the FSM.
        /// </remarks>
        public void Update()
        {
            if (IsDestroyed || IsPendingDestruction || Context == null || !Context.IsValid)
            {
                return;
            }

            try
            {
                if (!HasEnteredCurrentState)
                {
                    var currentState = Definition.GetState(CurrentStateID);
                    if (currentState != null)
                    {
                        currentState.Enter(Context);

                        // Entry callbacks can unregister this definition and destroy
                        // the handle. Do not continue into the step after teardown.
                        if (IsDestroyed || IsPendingDestruction)
                        {
                            return;
                        }

                        HasEnteredCurrentState = true;

                        // Entry is a user callback. If it suspends the context, do not
                        // execute Update or evaluate transitions during this step.
                        if (!Context.IsValid)
                        {
                            return;
                        }
                    }
                }

                var previousStateID = CurrentStateID;
                var nextStateID = Definition.Step(CurrentStateID, Context, this);
                if (IsDestroyed || IsPendingDestruction)
                {
                    return;
                }

                CurrentStateID = nextStateID;
                if (CurrentStateID != previousStateID)
                {
                    // Match the string-backed lifecycle: the target state's Enter action
                    // runs at the beginning of the next update, not during this transition.
                    HasEnteredCurrentState = false;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Integer FSMHandle {FSM_ID} failed during update.", ex);
            }
        }

        /// <summary>
        /// Manually evaluates transition conditions without running the current state's Update action.
        /// </summary>
        public void EvaluateConditions()
        {
            // Match the string-backed handle: an invalid caller-owned context suspends
            // evaluation without changing state or lifecycle bookkeeping.
            if (IsDestroyed || IsPendingDestruction || Context == null || !Context.IsValid)
            {
                return;
            }

            var nextStateID = Definition.EvaluateConditions(
                CurrentStateID, Context, out var transitioned, out var currentStateExited, this);
            if (IsDestroyed || IsPendingDestruction)
            {
                return;
            }

            CurrentStateID = nextStateID;
            if (currentStateExited)
            {
                HasEnteredCurrentState = false;
            }
            else if (transitioned)
            {
                HasEnteredCurrentState = true;
            }
        }

        /// <summary>Forces an immediate transition to an integer state identity.</summary>
        public void TransitionTo(int nextStateID)
        {
            if (IsDestroyed || IsPendingDestruction || Context == null || !Context.IsValid)
            {
                return;
            }

            try
            {
                if (!Definition.ForceTransition(CurrentStateID, nextStateID, Context, this))
                {
                    return;
                }
                CurrentStateID = nextStateID;
                HasEnteredCurrentState = true;
            }
            catch (Exception ex)
            {
                FSM_API.Error.InvokeInternalApiError($"Transition to integer state {nextStateID} failed for FSM handle {Id}.", ex);
                throw;
            }
        }

        /// <summary>Resets this instance to the blueprint's initial state.</summary>
        public void ResetFSMInstance()
        {
            if (IsDestroyed || IsPendingDestruction || Context == null || !Context.IsValid)
            {
                return;
            }

            if (!Definition.ForceTransition(CurrentStateID, Definition.InitialStateID, Context, this))
            {
                return;
            }
            CurrentStateID = Definition.InitialStateID;
            HasEnteredCurrentState = true;
        }

        /// <summary>Invokes this handle's Exit callback while preventing same-handle reentrant duplication.</summary>
        internal void ExitState(FSMState state)
        {
            if (state == null || _isExecutingExitCallback)
            {
                return;
            }

            _isExecutingExitCallback = true;
            try
            {
                state.Exit(Context);
            }
            finally
            {
                _isExecutingExitCallback = false;
            }
        }

        /// <summary>Runs the current state's Exit callback exactly once during teardown.</summary>
        internal void DestroyHandle()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;
            IsPendingDestruction = false;
            HasEnteredCurrentState = false;

            // If teardown was triggered from this handle's own Exit callback, that
            // callback is already the lifecycle exit for this state. Do not invoke it again.
            var wasExecutingExitCallback = _isExecutingExitCallback;
            var state = Definition.GetState(CurrentStateID);
            if (!wasExecutingExitCallback && state != null && Context != null)
            {
                try
                {
                    state.Exit(Context);
                }
                catch (Exception ex)
                {
                    FSM_API.Error.InvokeInternalApiError(
                        $"Error during teardown of integer FSM '{FSM_ID}' handle '{Id}' in state '{CurrentStateID}'.",
                        ex);
                }
            }
        }
    }
}