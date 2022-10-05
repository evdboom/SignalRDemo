using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Components.Game
{
    public record Recipient
    {
        public string Target { get; set; } = string.Empty;
        public TargetType TargetType { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Name => GetName();
        public string Id => $"{Target}_{TargetType}";
            
        private string GetName()
        {
            return TargetType switch
            {
                TargetType.All => "Everyone",
                TargetType.Group => $"Everyone in {Target} group",
                _ => $"{Target} ({Group})"
            };            
        }
    }                
}
