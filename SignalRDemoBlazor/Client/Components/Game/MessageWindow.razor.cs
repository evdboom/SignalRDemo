using Microsoft.AspNetCore.Components;
using SignalRDemoBlazor.Client.Components.Buttons;
using SignalRDemoBlazor.Client.Services;
using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Components.Game
{
    public partial class MessageWindow
    {
        [Inject]
        private SignalRService SignalRService { get; set; } = null!; 

        private List<Recipient> Recipients { get; set; } = new();
        private string Message { get; set; } = string.Empty;

        private Recipient? _selectedRecipient;
        private string Recipient 
        {
            get => _selectedRecipient?.Id ?? string.Empty;
            set => _selectedRecipient = Recipients.FirstOrDefault(x => x.Id == value);
        }
        private ButtonClass SendButton { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
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

        private async Task SendMessage()
        {
            if (_selectedRecipient is null)
            {
                return;
            }

            await SignalRService.SendMessage(Message, _selectedRecipient.Target, _selectedRecipient.TargetType);
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
            StateHasChanged();
        }


    }
}
