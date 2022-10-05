namespace SignalRDemo.Game
{
    public interface IGameManager
    {
        bool Started { get; }

        List<string> GetElements();
        string GetGameHostConnection();
        string GetGameHostName();
        IList<string> GetHint();
        string GetPassword();
        User? GetUser(string connectionId);
        bool IsValidColor(string color);
        IEnumerable<string> OtherTeams(string team);
        User RegisterUser(string userName, string connectionId);
        void StartGame(string connectionId, string password, string passwordHint, string winElementId, string winColor, string elementHint);
        void StopGame();
        bool SuccesfullChange(string connectionId, string elementId, string color);
        bool TryGetAllClients(string key, out IList<User> users);
        bool TryRemove(string connectionId, out User? user);
    }
}
