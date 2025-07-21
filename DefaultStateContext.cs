namespace TheSingularityWorkshop.FSM_API
{
    internal class DefaultStateContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "DefaultStateContext";
    }
}