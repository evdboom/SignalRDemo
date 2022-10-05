using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Server.Services
{
    public class GameManager : IGameManager
    {
        private readonly List<GameUser> _users;
        private readonly List<GameUser> _removedUsers;
        private readonly string[] _groups;
        private const string PinCode = "918273";

        private int _lastGroup;

        public GameManager()
        {
            _removedUsers = new();
            _users = new();
            _groups = new[]
            {
                "red",
                "green",
                "blue"
            };
            _lastGroup = -1;
        }

        public List<GameUser> GetUsers(string excludeConnectionId)
        {
            return _users
                .Where(user => user.ConnectionId != excludeConnectionId)
                .ToList();
        }

        public bool CanRegister(string userName, string pinCode, string connectionId, out string message)
        {
            if (pinCode != PinCode)
            {
                message = $"Incorrect PIN code";
            }
            else if (_users.Any(user => user.Name.ToLower() == userName.ToLower()))
            {
                message = $"Username {userName} is already taken.";
            }
            else if (_users.Any(user => user.ConnectionId == connectionId))
            {
                message = $"You are already connected, reload page in a new tab to force a new connection";
            }
            else
            {
                message = string.Empty;
            }

            return string.IsNullOrEmpty(message);
        }

        public GameUser? FindUser(string userName)
        {
            return _users.SingleOrDefault(user => user.Name.ToLower() == userName.ToLower());
        }

        public GameUser? FindUserByConnection(string connectionId)
        {
            return _users.SingleOrDefault(user => user.ConnectionId == connectionId);

        }

        public GameUser? ReregisterUser(string userName, string connectionId)
        {
            if (_removedUsers.FirstOrDefault(user => user.Name == userName) is GameUser reconnect && FindUser(userName) is null)
            {
                reconnect.ConnectionId = connectionId;
                _users.Add(reconnect);
                _removedUsers.Remove(reconnect);

                return reconnect;
            }

            return null;
        }

        public GameUser RegisterUser(string userName, string connectionId)
        {
            if (!CanRegister(userName, PinCode, connectionId, out string message))
            {
                throw new InvalidOperationException(message);
            }

            var user = new GameUser
            {
                ConnectionId = connectionId,
                Group = GetNextGroup(),
                Name = userName
            };
            _users.Add(user);

            return user;
        }

        public GameUser? RemoveUser(string connectionId)
        {
            var user = FindUserByConnection(connectionId);
            if (user is not null)
            {
                _users.Remove(user);
                _removedUsers.Add(user);
            }

            return user;
        }

        private string GetNextGroup()
        {
            _lastGroup++;
            if (_lastGroup >= _groups.Length)
            {
                _lastGroup = 0;
            }
            return _groups[_lastGroup];
        }        
    }
}
