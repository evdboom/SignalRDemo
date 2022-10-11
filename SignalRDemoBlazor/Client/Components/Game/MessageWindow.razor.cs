using Microsoft.AspNetCore.Components;
using SignalRDemoBlazor.Client.Components.Buttons;
using SignalRDemoBlazor.Client.Components.Messaging;
using SignalRDemoBlazor.Client.Events;
using SignalRDemoBlazor.Client.Services;
using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Components.Game
{
    public partial class MessageWindow
    {
        [Inject]
        private SignalRService SignalRService { get; set; } = null!;
        [Inject]
        private ScrollService ScrollService { get; set; } = null!;

        private List<MessageClass> Messages { get; set; } = new();
        private List<MessageClass> _unseenMessages = new();

        private List<Recipient> Recipients { get; set; } = new();
        private string Message { get; set; } = string.Empty;
        private ElementReference MessageArea { get; set; }


        private Recipient? _selectedRecipient;
        private string Recipient 
        {
            get => _selectedRecipient?.Id ?? string.Empty;
            set => _selectedRecipient = Recipients.FirstOrDefault(x => x.Id == value);
        }
        private ButtonClass SendButton { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            SignalRService.MessageReceived += MessageReceived;
            SignalRService.MessageAdded += MessageAdded;
            SignalRService.UsersChanged += UsersChanged;

            await GetRecipients();
            SendButton = new()
            {
                ActionType = ActionType.Confirm,
                ButtonType = ButtonTypes.Full,
                DisabledCondition = () => string.IsNullOrEmpty(Message) || _selectedRecipient is null,
                Name = "Send",
                ClickAction = async args => await SendMessage()
            };
 
        }

        public async Task Refresh()
        {
            if (_unseenMessages.Any())
            {
                Messages.AddRange(_unseenMessages);
                Messages = Messages
                    .OrderBy(m => m.ReceiveDateTime)
                    .ToList();
                _unseenMessages.Clear();
                await ScrollService.ScrollToBottom(MessageArea);
                StateHasChanged();
            }
        }

        private void MessageAdded(object? sender, MessageEventArgs e)
        {
            _unseenMessages.Add(new MessageClass
            {
                Body = e.Message.Body,
                Title = e.Message.Title,
                Type = e.Message.Type,
                ReceiveDateTime = DateTime.Now,
                OpenTime = e.Message.OpenTime
            });
        }

        private async void MessageReceived(object? sender, MessageEventArgs e)
        {
            Messages.Add(new MessageClass
            {
                Body = e.Message.Body,
                Title = e.Message.Title,
                Type = e.Message.Type,
                ReceiveDateTime = DateTime.Now,
                OpenTime = e.Message.OpenTime
            });
            await ScrollService.ScrollToBottom(MessageArea);
            StateHasChanged();
        }

        private async Task SendMessage()
        {
            if (_selectedRecipient is null || string.IsNullOrEmpty(Message))
            {
                return;
            }

            await SignalRService.SendMessage(Message, _selectedRecipient.Target, _selectedRecipient.TargetType);
            Message = string.Empty;
            StateHasChanged();
        }

        private string GetBorder(MessageClass message)
        {
            return message.Type switch
            {
                AlertType.Warning => "bg-warning border-warning",
                AlertType.Success => "bg-success border-success",
                AlertType.Danger => "bg-danger border-danger",
                _ => "bg-secondary border-secondary"
            };
        }

        private async void UsersChanged(object? sender, EventArgs e)
        {
            await GetRecipients();
        }

        private async Task GetRecipients()
        {
            Recipients.Clear();
            var users = await SignalRService.GetUsers();
            var groups = users
                .GroupBy(user => user.Group)
                .Select(group => new Recipient
                {
                    Target = group.Key,
                    TargetType = TargetType.Group
                });
            Recipients.Add(new Recipient { Target = "All", TargetType = TargetType.All });
            Recipients.AddRange(groups);
            Recipients.AddRange(users
                .Select(user => new Recipient
                {
                    Target = user.Name,
                    Group = user.Group,
                    TargetType = TargetType.User
                }));
            if (_selectedRecipient == null || !Recipients.Any(r => r.Id == _selectedRecipient.Id))
            {
                _selectedRecipient = Recipients.FirstOrDefault();
            }
            
            StateHasChanged();
        }


    }
}
