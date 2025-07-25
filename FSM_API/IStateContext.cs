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
    /// The `IStateContext` interface ensures your custom data model is compatible with the FSM system.
    /// It requires the implementation of an `IsValid` property, which the FSM uses to manage its lifecycle.
    /// </remarks>
    public interface IStateContext : IContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether this context object is currently valid and active.
        /// This is crucial for the FSM system to determine if an FSM instance associated with this context
        /// should continue to be processed.
        /// <para>
        /// **For Non-Coders/Domain Experts:**
        /// This property tells the FSM if the "thing" it's controlling is still relevant or active.
        /// If <c>IsValid</c> becomes <c>false</c>, the FSM will stop processing this particular instance.
        /// For example, if your "Light Switch" is physically removed, <c>IsValid</c> might become <c>false</c>.
        /// </para>
        /// <para>
        /// **For Developers:**
        /// Your implementation of <c>IsValid</c> should reflect the active status of the underlying object.
        /// For Unity, this might involve checking if the GameObject or MonoBehaviour is still active and not destroyed.
        /// For plain C# objects, this might involve checking if it has been explicitly "disposed",
        /// marked as invalid, or if its dependencies are no longer available.
        /// The FSM system relies on this property to prevent processing FSM instances tied to defunct contexts,
        /// thereby ensuring error-tolerant FSM lifecycle management.
        /// </para>
        /// </summary>
        /// <value><c>true</c> if the context is valid and active; otherwise, <c>false</c>.</value>

        bool IsValid { get; set; }
    }
}