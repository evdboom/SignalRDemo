using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SignalRDemoBlazor.Client.Components.Buttons;
using SignalRDemoBlazor.Client.Components.Messaging;
using SignalRDemoBlazor.Client.Services;
using SignalRDemoBlazor.Shared;
using System.Text.RegularExpressions;

namespace SignalRDemoBlazor.Client.Pages
{
    public partial class Index
    {
        [Inject]
        private SignalRService SignaRService { get; set; } = null!;
        [Inject]
        private MessageService MessageService { get; set; } = null!;

        private bool Registered { get; set; }
        private string UserName { get; set; } = string.Empty;
        private string PinCode { get; set; } = string.Empty;
        private string Group { get; set; } = string.Empty;
        private ButtonClass RegisterButton { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            RegisterButton = new()
            {
                ActionType = ActionType.Confirm,
                ButtonType = ButtonTypes.Full,
                ClickAction = async args => await RegisterUser(),
                Name = "Register",
                DisabledCondition = () => string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(PinCode)
            };

            SignaRService.MessageReceived += MessageReceived;
            await SignaRService.InitializeAsync();            
        }

        private async Task KeyPress(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await RegisterUser();
            }
        }


        private async Task RegisterUser()
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(PinCode))
            {
                return;
            }

            await SignaRService.RegisterUser(UserName, PinCode);
        }        

        private void MessageReceived(object? sender, Events.MessageEventArgs e)
        {
            
            if (!Registered && e.Message.Code == MessageCodes.SuccesfullyJoined)
            {

                Group = e.Message!.User!.Group;
                UserName = e.Message!.User!.Name;
                Registered = true;
                StateHasChanged();
            }

            MessageService.DisplayMessage(e.Message.Title, e.Message.Body, e.Message.Type);
        }
    }
}
