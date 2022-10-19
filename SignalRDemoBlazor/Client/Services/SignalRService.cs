using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRDemoBlazor.Client.Components.Messaging;
using SignalRDemoBlazor.Client.Events;
using SignalRDemoBlazor.Client.Services.Storage;
using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Services
{
    public class SignalRService
    {
        private const string ChatMessages = "ChatMessages";
        private const string User = "User";
        private const string Users = "Users";

        private readonly NavigationManager _navigation;
        private readonly SessionStorageService _storage;

        private HubConnection? _connection;
        private bool _disabled;
        private bool _amHost;

        public event EventHandler<MessageEventArgs>? MessageAdded;
        public event EventHandler<MessageEventArgs>? MessageReceived;
        public event EventHandler<MessageEventArgs>? ChatReceived;
        public event EventHandler? UsersChanged;
        public event EventHandler<QuestionEventArgs>? QuestionAsked;
        public event EventHandler<ProgressEventArgs>? QuestionProgressUpdated;
        public event EventHandler<AnswerEventArgs>? QuestionDone;
        public event EventHandler? QuestionStarted;
        public event EventHandler<bool>? MayEnable;

        public bool AmHost => _amHost;
        public bool Disabled => _disabled;
        public bool Connected => _connection?.State == HubConnectionState.Connected;


        public SignalRService(NavigationManager navigation, SessionStorageService storage)
        {
            _navigation = navigation;
            _storage = storage;
        }

        public async Task InitializeAsync()
        {
            await _storage.SetItemIfEmptyAsync<List<MessageClass>>(ChatMessages, new());
            await _storage.SetItemIfEmptyAsync<List<GameUser>>(Users, new());

            _connection = new HubConnectionBuilder()
                .WithUrl(_navigation.ToAbsoluteUri("/demoHub"))
                .Build();

            _connection.On(MessageType.MessageReceived, async (string message, GameUser sender, AlertType type, string? messageCode) =>
            {                
                var totalMessage = new MessageClass
                {
                    Title = (sender.IsSystemUser || 
                            messageCode == MessageCodes.SuccesfullyJoined) 
                        ? "System" 
                        : $"{sender?.Name ?? "Someone"} sent you a message",
                    Body = message,
                    Type = type,
                    Code = messageCode,
                    User = sender
                };

                if (messageCode == MessageCodes.SuccesfullyJoined)
                {
                    await _storage.SetItemAsync(User, sender!);
                    _amHost = sender!.IsGameHost;
                }

                if (!_disabled)
                {
                    MessageReceived?.Invoke(this, new MessageEventArgs(totalMessage));
                }
                else
                {
                    MessageAdded?.Invoke(this, new MessageEventArgs(totalMessage));
                }
            });
            _connection.On(MessageType.ChatReceived, async (string message, GameUser sender) =>
            {
                var totalMessage = new MessageClass
                {
                    Title = sender.IsSystemUser ? "System" : (sender?.Name ?? "Someone"),
                    Body = message,
                    User = sender
                };
                var messages = await _storage.GetItemAsync<List<MessageClass>>(ChatMessages);
                messages?.Add(totalMessage);
                await _storage.SetItemAsync(ChatMessages, messages);
                if (!_disabled)
                {
                    ChatReceived?.Invoke(this, new MessageEventArgs(totalMessage));
                }
            });
            _connection.On(MessageType.UserJoined, async (GameUser user) =>
            {
                var users = await _storage.GetItemAsync<List<GameUser>>(Users);
                users!.Add(user);
                await _storage.SetItemAsync(Users, users);
                if (!_disabled)
                {
                    UsersChanged?.Invoke(this, new());
                }

            });
            _connection.On(MessageType.UserLeft, async (GameUser user) =>
            {
                var users = await _storage.GetItemAsync<List<GameUser>>(Users);
                users!.Remove(user);
                await _storage.SetItemAsync(Users, users);
                if (!_disabled)
                {
                    UsersChanged?.Invoke(this, new());
                }

            });
            _connection.On(MessageType.QuestionReceived, (Question question) =>
            {
                QuestionAsked?.Invoke(this, new QuestionEventArgs(question));
            });
            _connection.On(MessageType.QuestionDone, (AnswerResult answer) =>
            {
                QuestionDone?.Invoke(this, new(answer));
            });
            _connection.On(MessageType.QuestionStarted, async () =>
            {
                QuestionStarted?.Invoke(this, new());
                var stream = _connection.StreamAsync<QuestionProgress>(MessageType.QuestionProgress);
                await foreach (var progress in stream)
                {
                    QuestionProgressUpdated?.Invoke(this, new ProgressEventArgs { Loaded = progress.CurrentProgress, Total = progress.TotalProgress });
                }
            });
            _connection.On(MessageType.MayEnable, (bool mayEnable) =>
            {
                if (!mayEnable)
                {
                    Disable();
                }
                MayEnable?.Invoke(this, mayEnable);
            });

            await _connection.StartAsync();

            await RefreshUsers();
            await Reregister();
        }

        public async Task<bool> GetMayEnable()
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }

            var mayEnable = await _connection!.InvokeAsync<bool>(MessageType.GetMayEnable);
            if (!mayEnable)
            {
                Disable();
            }
            return mayEnable;
        }

        public async Task RefreshUsers()
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }

            var users = await _connection!.InvokeAsync<List<GameUser>>(MessageType.GetUserList);
            await _storage.SetItemAsync(Users, users);
            if (!_disabled)
            {
                UsersChanged?.Invoke(this, new());
            }
        }

        public async Task<List<GameUser>> GetUsers()
        {
            return await _storage.GetItemAsync<List<GameUser>>(Users) ?? new List<GameUser>();
        }

        public async Task<List<MessageClass>> GetChat()
        {
            return await _storage.GetItemAsync<List<MessageClass>>(ChatMessages) ?? new List<MessageClass>();
        }

        private async Task Reregister()
        {
            var user = await _storage.GetItemAsync<GameUser>(User);
            if (user is not null)
            {
                await _connection!.SendAsync(MessageType.ReregisterUser, user.Name, user.ConnectionId);
            }
        }

        public async Task SendMessage(string message, string target, TargetType targetType)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }

            await _connection!.SendAsync(MessageType.SendMessage, message, target, targetType);
        }

        public async Task SendChat(string message)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }

            var messageClass = new MessageClass
            {
                Title = "You",
                Body = message
            };
            var messages = await _storage.GetItemAsync<List<MessageClass>>(ChatMessages);
            messages?.Add(messageClass);
            await _storage.SetItemAsync(ChatMessages, messages);
            ChatReceived?.Invoke(this, new MessageEventArgs(messageClass));

            await _connection!.SendAsync(MessageType.SendChat, message);
        }

        public void Disable()
        {
            _disabled = true;
        }

        public void Enable()
        {
            _disabled = false;
        }

        public async Task RegisterUser(string userName, string pinCode)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }


            await _connection!.SendAsync(MessageType.RegisterUser, userName, pinCode);
        }

        public Task Refresh()
        {
            UsersChanged?.Invoke(this, new());
            ChatReceived?.Invoke(this, new MessageEventArgs(null!));

            return Task.CompletedTask;
        }

        public async Task AnswerQuestion(string question, string answer)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }

            await _connection!.SendAsync(MessageType.AnswerQuestion, question, answer);
        }

        public async Task StartQuiz()
        {
            await _connection!.SendAsync(MessageType.StartQuiz);
        }

        public async Task StartQuestion()
        {
            await _connection!.SendAsync(MessageType.StartQuestion);
        }

        public async Task GetState()
        {
            var question = await _connection!.InvokeAsync<Question>(MessageType.GetState);
            if (question is null)
            {
                return;
            }

            QuestionAsked?.Invoke(this, new(question));
            if (question.AskTime > DateTime.MinValue)
            {
                QuestionStarted?.Invoke(this, new());
                var stream = _connection!.StreamAsync<QuestionProgress>(MessageType.QuestionProgress);
                await foreach (var progress in stream)
                {
                    QuestionProgressUpdated?.Invoke(this, new ProgressEventArgs { Loaded = progress.CurrentProgress, Total = progress.TotalProgress });
                }
            }
        }

        public async Task ActivateSignalR()
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }

            await _connection!.SendAsync(MessageType.ActivateSignalR);
        }

        public async Task ResetGame()
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Hub not connected yet, did you initialize?");
            }

            await _connection!.SendAsync(MessageType.ResetGame);
        }
    }
}
