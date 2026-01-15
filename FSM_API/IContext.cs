namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Base interface for all fundamental context objects within The Singularity Workshop's API ecosystem.
    /// <para>
    /// This interface provides a common contract for any object that needs a unique name for identification,
    /// debugging, or management purposes across various systems, including but not limited to Finite State Machines.
    /// It serves as a foundational building block, allowing for consistent naming conventions for entities
    /// that are processed, tracked, or referenced by higher-level abstractions.
    /// </para>
    /// <para>
    /// While primarily used as the base for <see cref="IStateContext"/> within the FSM API,
    /// <c>IContext</c> is designed to be more general. It lays the groundwork for future modular components
    /// (e.g., "Atomic" operations, processing units, or other managed entities) that require
    /// a common identity but may not necessarily involve state transitions.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Implementing <c>IContext</c> ensures that an object can be easily identified and referenced
    /// by name within diagnostic tools, logs, or management interfaces, promoting clarity and traceability
    /// throughout your application.
    /// </remarks>
    public interface IContext
    {
        /// <summary>
        /// Gets or sets the unique name of the context object.
        /// This name is used for identification, debugging, logging, and potentially for grouping or lookup
        /// within systems that manage collections of contexts.
        /// </summary>
        /// <value>A string representing the descriptive name of the context instance.</value>

        string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Refactored to hash V1.0.14
        /// </remarks>
        int Context_ID { get; }
    }
}