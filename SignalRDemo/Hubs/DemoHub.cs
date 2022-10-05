using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Game;
using System.Drawing;

namespace SignalRDemo.Hubs
{
    public class DemoHub : Hub
    {
        private readonly IGameManager _gameManager;

        public DemoHub(IGameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public async Task GetElements()
        {
            await Clients
                .Caller
                .SendAsync("Elements", _gameManager.GetElements());
        }

        public async Task ChangeColor(string elementId, string color, string connectionId)
        {
            if (!_gameManager.Started)
            {
                await Clients
                    .Client(Context.ConnectionId)
                    .SendAsync("GameNotStarted");
                return;
            }

            if (!_gameManager.IsValidColor(color))
            {
                return;
            }

            var user = _gameManager.GetUser(Context.ConnectionId);
            var target = _gameManager.GetUser(connectionId);

            if (user is null)
            {
                return;
            }

            if (target is null)
            {
                await Clients
                    .Caller
                    .SendAsync("UnknownUser", connectionId);
                return;
            }

            await Clients
                .Client(connectionId)
                .SendAsync("ChangeColor", elementId, color, user);
            await Clients
                .Clients(_gameManager.GetGameHostConnection(), Context.ConnectionId)
                .SendAsync("LogColor", target, color, elementId, user);

            if (_gameManager.SuccesfullChange(connectionId, elementId, color))
            {
                _gameManager.StopGame();
                await Clients
                    .Clients(_gameManager.GetGameHostConnection())
                    .SendAsync("LogWon", _gameManager.GetPassword(), target, color, elementId, user);

                await Clients
                    .Group(user.Team)
                    .SendAsync("Won", _gameManager.GetPassword(), target, color, elementId, user);
                foreach (var team in _gameManager.OtherTeams(user.Team))
                {
                    await Clients
                        .Group(team)
                        .SendAsync("Lost", _gameManager.GetPassword(), target, color, elementId, user);
                }
                _gameManager.StopGame();
            }
        }

        public async Task GetHint()
        {
            if (!_gameManager.Started)
            {
                await Clients
                    .Caller
                    .SendAsync("GameNotStarted");
                return;
            }

            await Clients
                .Caller
                .SendAsync("Hint", _gameManager.GetHint());
        }

        public async Task StartGame(string password, string passwordHint, string winElementId, string winColor, string elementHint)
        {
            if (!_gameManager.IsValidColor(winColor))
            {
                return;
            }

            if (!_gameManager.Started)
            {
                _gameManager.StartGame(Context.ConnectionId, password, passwordHint, winElementId, winColor, elementHint);
                await Clients
                    .All
                    .SendAsync("GameStarted", _gameManager.GetGameHostName());
            }
            else
            {
                await Clients
                    .Caller
                    .SendAsync("GameAlreadyStarted", _gameManager.GetGameHostName());
            }
        }

        public async Task GetClients(string key)
        {
            if (!_gameManager.Started)
            {
                await Clients
                    .Caller
                    .SendAsync("GameNotStarted");
                return;
            }

            var user = _gameManager.GetUser(Context.ConnectionId);

            if (user is null)
            {
                return;
            }

            if (_gameManager.TryGetAllClients(key, out var users))
            {
                await Clients
                    .Group(user.Team)
                    .SendAsync("ClientList", users, true, user);
                foreach (var team in _gameManager.OtherTeams(user.Team))
                {
                    await Clients
                    .Group(team)
                        .SendAsync("GotAllClients", user.Team);
                    await Clients
                        .Client(_gameManager.GetGameHostConnection())
                        .SendAsync("GotAllClients", user.Team);
                }
            }
            else
            {
                await Clients
                    .Caller
                    .SendAsync("ClientList", users, false);
                await Clients
                    .Group(user.Team)
                    .SendAsync("LogPassword", user, key);
                await Clients
                    .Client(_gameManager.GetGameHostConnection())
                    .SendAsync("LogPassword", user, key);
            }
        }

        public async Task RegisterUser(string userName)
        {
            var user = _gameManager.RegisterUser(userName, Context.ConnectionId);
            if (user is null)
            {
                return;
            }

            await Groups.AddToGroupAsync(user.ConnectionId, user.Team);
            await Clients
                .All
                .SendAsync("UserRegistered", user);

            await Clients
                .Group(user.Team)
                .SendAsync("UserInTeam", user);

            await Clients
                .Caller
                .SendAsync("Enable", user.Team, _gameManager.Started);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_gameManager.TryRemove(Context.ConnectionId, out User? user))
            {
                await Clients
                    .All
                    .SendAsync("UserLeft", user);
            }
        }
    }
}
