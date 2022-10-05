using Microsoft.AspNetCore.Components;
using SignalRDemoBlazor.Client.Components.Buttons;
using SignalRDemoBlazor.Client.Services;

namespace SignalRDemoBlazor.Client.Components.Game
{
    public partial class GameMain
    {
        [Parameter]
        public string Group { get; set; } = string.Empty;
        [Parameter]
        public string UserName { get; set; } = string.Empty;

        [Inject]
        private SignalRService SignaRService { get; set; } = null!;

        private ButtonClass RefreshButton { get; set; } = null!;

        protected override void OnInitialized()
        {
            RefreshButton = new()
            {
                ActionType = ActionType.Refresh,
                ButtonType = ButtonTypes.Full,
                Name = "Refresh",
                ClickAction = async args => await Refresh()
            };
            base.OnInitialized();
        }

        private async Task Refresh()
        {
            await SignaRService.Refresh();
        }

        private string GetStyle()
        {
            if (string.IsNullOrEmpty(Group))
            {
                return string.Empty;
            }

            return $"color: {Group}";
        }

        private void Enable()
        {
            SignaRService.Enable();
        }

        private void Disable()
        {
            SignaRService.Disable();
        }
    }
}
