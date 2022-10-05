using System.Text.RegularExpressions;

namespace SignalRDemo.Game
{
    public class GameManager : IGameManager
    {
        private readonly List<User> _users;        
        private readonly List<string> _hints;
        private readonly string[] _teams;
        private readonly List<string> _elements = new()
        {

        };


        private int _lastTeam;

        private User? _gameHost;
        private string _password = string.Empty;
        private string _elementId = string.Empty;
        private string _color = string.Empty;

        public bool Started { get; private set; }

        public GameManager()
        {
            _lastTeam = -1;
            _users = new();
            _hints = new();
            _teams = new[]
            {
                "Red",
                "Green",
                "Blue"
            };
        }
        
        public string GetGameHostConnection()
        {
            return _gameHost?.ConnectionId ?? string.Empty;
        }

        public string GetGameHostName()
        {
            return _gameHost?.Name ?? string.Empty;
        }

        public IList<string> GetHint()
        {
            return _hints;
        }

        public string GetPassword()
        {
            return _password;
        }

        public User? GetUser(string connectionId)
        {
            return _users
                .FirstOrDefault(user => user.ConnectionId == connectionId);
        }

        public IEnumerable<string> OtherTeams(string team)
        {
            return _teams
                .Where(t => t != team);
        }

        public User RegisterUser(string userName, string connectionId)
        {
            if (_users.SingleOrDefault(user => user.Name == userName) is not null)
            {
                throw new ArgumentException($"Username {userName} already taken");
            }
            else if (_users.SingleOrDefault(user => user.ConnectionId == connectionId) is User known)
            {
                throw new ArgumentException($"Already registered as user {known.Name}");
            }

            var user = new User
            {
                ConnectionId = connectionId,
                Name = userName,
                Team = GetNextTeam()
            };

            _users.Add(user);
            return user;            
        }

        public void StartGame(string connectionId, string password, string passwordHint, string winElementId, string winColor, string elementHint)
        {
            if (Started)
            {
                throw new InvalidOperationException("Game already started");
            }
            var gameHost = _users.SingleOrDefault(user => user.ConnectionId == connectionId);

            if (gameHost is null)
            {
                throw new InvalidOperationException("Game cannot be started by unknown user");
            }

            Started = true;
            _gameHost = gameHost;
            _password = password;
            _elementId = winElementId;
            _color = winColor;
            _hints.AddRange(new[]
            {
                passwordHint,
                elementHint
            });
        }

        public void StopGame()
        {
            Started = false;
            _gameHost = null;
            _password = string.Empty;
            _elementId = string.Empty;
            _color = string.Empty;
            _hints.Clear();
        }

        public bool SuccesfullChange(string connectionId, string elementId, string color)
        {
            return
                connectionId == _gameHost?.ConnectionId &&
                elementId == _elementId &&
                color == _color;
                    
        }

        public bool TryGetAllClients(string key, out IList<User> users)
        {
            var found = key == _password;
            users = found
                ? _users
                : _users
                    .Where(user => user.ConnectionId != _gameHost?.ConnectionId)
                    .ToList();

            return found;
        }

        public bool TryRemove(string connectionId, out User? user)
        {
            user = GetUser(connectionId);
            if (user is null)
            {
                return false;
            }

            _users.Remove(user);

            if (connectionId == _gameHost?.ConnectionId)
            {
                StopGame();               
                throw new InvalidOperationException("Gamehost left!");
            }

            return true;
        }

        private string GetNextTeam()
        {
            _lastTeam++;
            if (_lastTeam >= _teams.Length)
            {
                _lastTeam = 0;
            }
            return _teams[_lastTeam];
        }

        public List<string> GetElements()
        {
            return _elements;
        }

        public bool IsValidColor(string color)
        {
            if (Regex.Match(color, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                return true;

            var result = System.Drawing.Color.FromName(color);
            return result.IsKnownColor;
        }
    }
}
