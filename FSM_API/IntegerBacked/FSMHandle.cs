using System;

namespace TheSingularityWorkshop.FSM_API.IntegerBacked
{
    /// <summary>
    /// Runtime handle for an integer-backed FSM instance.
    /// </summary>
    /// <remarks>
    /// The handle deliberately stores only integer FSM/state identities and the instance context.
    /// The hot Update path does not perform string state lookup or create temporary collections.
    /// </remarks>
    public sealed class FSMHandle
    {
        /// <summary>Unique identity of this running FSM instance.</summary>
        public int Id { get; internal set; }

        /// <summary>The integer-backed FSM blueprint controlled by this handle.</summary>
        public readonly FSM Definition;

        /// <summary>The FSM blueprint identity.</summary>
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
                FsmApi.Error.InvokeInstanceError(this, $"Integer FSMHandle: {FSM_ID} has crashed: {ex.Message}", ex, processGroup);
            }
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
                FsmApi.Error.InvokeInstanceError(this, $"Transition to: {nextStateID} failed.", ex);
                throw;
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
