using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Server.Services
{
    public interface IGameManager
    {
        bool CanRegister(string userName, string pinCode, string connectionId, out string message);
        GameUser? FindUser(string target);
        GameUser? FindUserByConnection(string connectionId);
        GameUser RegisterUser(string userName, string connectionId);
        GameUser? ReregisterUser(string userName, string connectionId);
        GameUser? RemoveUser(string connectionId);
        List<GameUser> GetUsers(string excludeConnectionId);
    }
}
