namespace SignalRDemoBlazor.Shared
{
    public record GameUser
    {
        public bool IsGameHost { get; set; }
        public bool IsSystemUser { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
    }
}
