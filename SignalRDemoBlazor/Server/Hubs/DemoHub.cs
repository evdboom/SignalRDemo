using Microsoft.AspNetCore.SignalR;
using SignalRDemoBlazor.Server.Services;
using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Server.Hubs
{
    public class DemoHub : Hub
    {
        private readonly IGameManager _gameManager;
        private readonly GameUser _systemUser;        

        public DemoHub(IGameManager gameManager)
        {
            _gameManager = gameManager;
            _systemUser = new GameUser()
            {
                Name = "System",
                IsSystemUser = true,
            };
        }

        public async Task GetUserList()
        {
            var users = _gameManager.GetUsers(Context.ConnectionId);
            await Clients
                .Caller
                .SendAsync(MessageType.UserListReceived, users);
        }

        public async Task ReregisterUser(string userName)
        {
            if (_gameManager.ReregisterUser(userName, Context.ConnectionId) is GameUser reconnect)
            {                                       
                await Groups.AddToGroupAsync(Context.ConnectionId, reconnect.Group);

                await Clients
                    .Caller
                    .SendAsync(MessageType.MessageReceived, $"You succesfully reconnected", reconnect, AlertType.Success, MessageCodes.SuccesfullyJoined);

                await Clients
                    .Others
                    .SendAsync(MessageType.MessageReceived, $"{reconnect.Name} just reconnected", _systemUser, AlertType.Information, "");
                await Clients
                    .Others
                    .SendAsync(MessageType.UserJoined, reconnect);
            }
        }

        public async Task RegisterUser(string userName, string pinCode)
        {
            if (_gameManager.CanRegister(userName, pinCode, Context.ConnectionId, out string message))
            {
                var user = _gameManager.RegisterUser(userName, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, user.Group);

                await Clients
                    .Caller
                    .SendAsync(MessageType.MessageReceived, $"You succesfully joined the {user.Group} group", user, AlertType.Success, MessageCodes.SuccesfullyJoined);

                await Clients
                    .Others
                    .SendAsync(MessageType.MessageReceived, $"{user.Name} just joined the {user.Group}", _systemUser, AlertType.Information, "");
                await Clients
                    .Others
                    .SendAsync(MessageType.UserJoined, user);

            }
            else
            {
                await Clients
                    .Caller
                    .SendAsync(MessageType.MessageReceived, $"Cannot register you as a user for the game, because: {message}", _systemUser, AlertType.Warning, "");
            }
        }

        public async Task ChatMessage(string message)
        {
            var sender = _gameManager.FindUserByConnection(Context.ConnectionId);
            if (sender is null)
            {
                await Clients
                    .Caller
                    .SendAsync(MessageType.MessageReceived, "You are not known a a user, did you register?", _systemUser, AlertType.Warning, "");
                return;
            }

            await Clients
                .Others
                .SendAsync(MessageType.ChatReceived, message, sender);
        }

        public Task SendMessage(string message, string target, TargetType targetType)
        {
            return Send(MessageType.MessageReceived, message, target, targetType);
        }

        private async Task Send(string sendMethod, string message, string target, TargetType targetType)
        {
            var sender = _gameManager.FindUserByConnection(Context.ConnectionId);
            if (sender is null)
            {
                await Clients
                    .Caller
                    .SendAsync(MessageType.MessageReceived, "You are not known a a user, did you register?", _systemUser, AlertType.Warning, "");
                return;
            }

            switch (targetType)
            {
                case TargetType.All:
                    await Clients
                        .Others
                        .SendAsync(sendMethod, message, sender, AlertType.Information, "");
                    break;
                case TargetType.Group:
                    await Clients
                        .Group(target)
                        .SendAsync(sendMethod, message, sender, AlertType.Information, "");
                    break;
                case TargetType.User:
                    var user = _gameManager.FindUser(target);
                    if (user is null)
                    {
                        await Clients
                            .Caller
                            .SendAsync(MessageType.MessageReceived, $"No user with username {target}", _systemUser, AlertType.Warning, "");
                        break;
                    }
                    await Clients
                        .Client(user.ConnectionId)
                        .SendAsync(sendMethod, message, sender, AlertType.Information, "");
                    break;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_gameManager.RemoveUser(Context.ConnectionId) is GameUser user)
            {
                await Clients
                    .All
                    .SendAsync(MessageType.MessageReceived, $"User {user.Name} just disconnected.", _systemUser, AlertType.Information, "");
                await Clients
                    .All
                    .SendAsync(MessageType.UserLeft, user);
            }
        }
    }
}
