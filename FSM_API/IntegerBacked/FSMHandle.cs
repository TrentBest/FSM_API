using System;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Runtime handle for an integer-backed FSM instance.
    /// </summary>
    /// <remarks>
    /// The handle deliberately stores only integer FSM/state identities and the instance context.
    /// The hot Update path does not perform string state lookup or create temporary allocations on the successful path.
    /// <para>
    /// <see cref="FSMHandleInt"/> is the explicit public handle type returned by the integer runtime.
    /// This base remains available so existing IntegerBacked callers continue to compile while the API
    /// establishes the representation-specific handle boundary.
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
        /// Initialization is explicit and idempotent. This keeps construction allocation-free while
        /// preserving the state lifecycle contract: the initial state's Enter action occurs exactly once
        /// until the instance is reset.
        /// </remarks>
        public void Initialize()
        {
            if (HasEnteredCurrentState)
            {
                return;
            }

            Definition.EnterInitial(Context);
            CurrentStateID = Definition.InitialStateID;
            HasEnteredCurrentState = true;
        }

        /// <summary>
        /// Advances this FSM instance by one step.
        /// </summary>
        /// <remarks>
        /// Normal execution uses integer identities throughout the path from the handle to the FSM,
        /// avoiding string-based state lookup and avoiding temporary allocations on the successful path.
        /// </remarks>
        public void Update(string processGroup = "Update")
        {
            try
            {
                CurrentStateID = Definition.Step(CurrentStateID, Context);
                HasEnteredCurrentState = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Integer FSMHandle {FSM_ID} failed during update.", ex);
            }
        }

        /// <summary>
        /// Manually evaluates transition conditions without running the current state's Update action.
        /// </summary>
        /// <remarks>
        /// This mirrors the string-backed manual evaluation path while keeping the current state identity
        /// entirely integer-backed. At most one transition is taken during an evaluation.
        /// </remarks>
        public void EvaluateConditions()
        {
            CurrentStateID = Definition.EvaluateConditions(CurrentStateID, Context);
            HasEnteredCurrentState = true;
        }

        /// <summary>Forces an immediate transition to an integer state identity.</summary>
        public void TransitionTo(int nextStateID)
        {
            try
            {
                Definition.ForceTransition(CurrentStateID, nextStateID, Context);
                CurrentStateID = nextStateID;
                HasEnteredCurrentState = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Transition to integer state {nextStateID} failed.", ex);
            }
        }

        /// <summary>Resets this instance to the blueprint's initial state.</summary>
        public void ResetFSMInstance()
        {
            Definition.ForceTransition(CurrentStateID, Definition.InitialStateID, Context);
            CurrentStateID = Definition.InitialStateID;
            HasEnteredCurrentState = true;
        }
    }
}
