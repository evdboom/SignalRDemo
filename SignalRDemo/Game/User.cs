namespace SignalRDemo.Game
{
    public record User
    {
        public string Name { get; init; } = string.Empty;
        public string Team { get; init; } = string.Empty;
        public string ConnectionId { get; init; } = string.Empty;
    }
}
