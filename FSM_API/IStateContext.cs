namespace TheSingularityWorkshop.FSM_API
{

    /// <summary>
    /// Represents the "Context" or "Data Model" for an FSM instance.
    /// <para>
    /// **For Non-Coders/Domain Experts:**
    /// Think of this as the "thing" or "object" whose behavior you want to control or describe using a Finite State Machine.
    /// For example, if you're building a "Light Switch" FSM, your <c>LightSwitch</c> class would implement this interface.
    /// It holds all the relevant data and properties that the FSM will read and potentially modify as it transitions between states.
    /// </para>
    /// <para>
    /// **For Developers:**
    /// Implement this interface on any Plain Old C# Object (POCO) that represents the data model
    /// or entity you want an FSM to manage. This design promotes a clean separation of concerns,
    /// decoupling your FSM logic from your application's core data. Your FSM definitions will
    /// operate directly on instances of this context.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The `IStateContext` interface ensures your caller-owned data model is compatible with the FSM system.
    /// Its `IsValid` property is a computation-eligibility gate, not a handle lifecycle command. FSM_API
    /// does not own, dispose, or invalidate the context; explicit instance APIs control handle registration.
    /// </remarks>
    public interface IStateContext : IContext
    {
        /// <summary>
        /// Gets or sets whether computation associated with this caller-owned context is currently permitted.
        /// When false, scheduled and manual stepping/condition evaluation are skipped. The handle remains
        /// registered and can resume if validity becomes true again. This property does not request handle
        /// destruction, unregister instances, or transfer ownership of the context to FSM_API.
        /// <para>
        /// **For Non-Coders/Domain Experts:**
        /// This property tells the FSM if the "thing" it's controlling is still relevant or active.
        /// If <c>IsValid</c> becomes <c>false</c>, the FSM instance is suspended: automatic processing skips it, but the registered handle and its state are retained.
        /// Set it back to <c>true</c> when the same context is reusable. Use explicit instance destruction when the handle should be removed.
        /// </para>
        /// <para>
        /// **For Developers:**
        /// Your implementation of <c>IsValid</c> should reflect whether FSM computation is currently safe.
        /// A false value suppresses computation for every handle sharing this context; it does not remove
        /// those handles. The caller remains responsible for the context object's lifetime, while FSM_API
        /// exposes explicit APIs for removing its own handle registrations.
        /// </para>
        /// </summary>
        /// <value><c>true</c> if the context is valid and active; otherwise, <c>false</c>.</value>

        bool IsValid { get; set; }
     
    }
}