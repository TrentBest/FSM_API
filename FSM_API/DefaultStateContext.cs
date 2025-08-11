namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Provides a default, basic implementation of the <see cref="IStateContext"/> interface.
    /// </summary>
    /// <remarks>
    /// This class is used internally by the FSM API when an <see cref="FSMHandle"/> is created
    /// without a specific <see cref="IStateContext"/> provided by the user.
    /// It ensures that every FSM instance always has a valid context object to operate on,
    /// even if that context contains no custom data or behavior.
    /// As an <c>internal</c> class, it is not intended for direct use or instantiation by external API consumers.
    /// </remarks>
    internal class DefaultStateContext : IStateContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether this context is considered valid.
        /// Always <c>true</c> for <see cref="DefaultStateContext"/>.
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of this default context.
        /// Always "DefaultStateContext".
        /// </summary>
        public string Name { get; set; } = "DefaultStateContext";
        public bool HasEnteredCurrentState { get; set; }
    }
}