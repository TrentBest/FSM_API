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

            Definition.EnterInitial(Context);
            CurrentStateID = Definition.InitialStateID;
            HasEnteredCurrentState = Definition.HasState(CurrentStateID);
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
                        HasEnteredCurrentState = true;
                    }
                }

                var previousStateID = CurrentStateID;
                CurrentStateID = Definition.Step(CurrentStateID, Context);
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

            CurrentStateID = Definition.EvaluateConditions(CurrentStateID, Context, out var transitioned);
            if (transitioned)
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
                Definition.ForceTransition(CurrentStateID, nextStateID, Context);
                CurrentStateID = nextStateID;
                HasEnteredCurrentState = Definition.HasState(nextStateID);
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

            Definition.ForceTransition(CurrentStateID, Definition.InitialStateID, Context);
            CurrentStateID = Definition.InitialStateID;
            HasEnteredCurrentState = true;
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

            var state = Definition.GetState(CurrentStateID);
            if (state != null && Context != null)
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