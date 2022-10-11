using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SignalRDemoBlazor.Client.Components.Buttons;
using SignalRDemoBlazor.Client.Components.Messaging;
using SignalRDemoBlazor.Client.Services;
using SignalRDemoBlazor.Shared;
using System.Text;

namespace SignalRDemoBlazor.Client.Components.Game
{
    public partial class ChatWindow
    {
        private const string MeTitle = "You";

        [Parameter]
        public string UserName { get; set; } = string.Empty;

        [Parameter]
        public string Group { get; set; } = string.Empty;

        [Inject]
        private SignalRService SignalRService { get; set; } = null!;
        [Inject]
        private ScrollService ScrollService { get; set; } = null!;

        private List<GameUser> Users { get; set; } = new();
        private string Message { get; set; } = string.Empty;
        private ButtonClass SendButton { get; set; } = null!;
        private List<MessageClass> Messages { get; set; } = new();
        private ElementReference ChatArea { get; set; }


        protected override async Task OnInitializedAsync()
        {
            SendButton = new()
            {
                ActionType = ActionType.Confirm,
                ButtonType = ButtonTypes.Icon,
                Name = "Send",
                DisabledCondition = () => string.IsNullOrEmpty(Message),
                ClickAction = async args => await SendMessage(),
                Size = ButtonSize.ExtraSmall
            };

            SignalRService.ChatReceived += ChatReceived;
            SignalRService.UsersChanged += UsersChanged;
            Messages = await SignalRService.GetChat();
            Users = await SignalRService.GetUsers();
        }

        private async Task KeyPress(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await SendMessage();
            }            
        }

        private string GetMessageClasses(MessageClass message)
        {
            StringBuilder sb = new();
            sb.Append("p-1 mb-2 border rounded-1");
            if (message.Title == MeTitle)
            {
                sb.Append(" border-success bg-success text-end");
            }
            else
            {
                sb.Append(" border-info bg-info");
            }
            sb.Append(" bg-opacity-25");

            return sb.ToString();
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrEmpty(Message))
            {
                return;
            }

            await SignalRService.SendChat(Message);
            Message = string.Empty;
            StateHasChanged();
        }

        private async void UsersChanged(object? sender, EventArgs e)
        {
            Users = await SignalRService.GetUsers();
            StateHasChanged();
        }

        private async void ChatReceived(object? sender, Events.MessageEventArgs e)
        {
            if (e.Message is null)
            {
                Messages = await SignalRService.GetChat();
            }
            else
            {
                Messages.Add(e.Message);
            }

            await ScrollService.ScrollToBottom(ChatArea);
            StateHasChanged();
         
        }
    }
}
