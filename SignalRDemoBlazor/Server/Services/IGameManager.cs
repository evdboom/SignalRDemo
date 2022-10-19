using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Server.Services
{
    public interface IGameManager
    {
        bool MayEnable { get; }
        void Enable();
        bool CanRegister(string userName, string pinCode, string connectionId, out string message);
        GameUser? FindUser(string target);
        GameUser? FindUserByConnection(string connectionId);
        GameUser RegisterUser(string userName, string connectionId, string pinCode);
        GameUser? ReregisterUser(string userName, string connectionId, string oldConnectionId);
        GameUser? RemoveUser(string connectionId);
        List<GameUser> GetUsers(string excludeConnectionId);
        void AnswerQuestion(GameUser sender, string question, string answer);
        Question? GetNextQuestion();
        Question? GetCurrentQuestion();
        void StartQuestion();
        string? GetHostId();
        void Reset();
    }
}
