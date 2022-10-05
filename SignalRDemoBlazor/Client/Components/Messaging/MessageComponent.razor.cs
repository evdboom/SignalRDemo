using Microsoft.AspNetCore.Components;

namespace SignalRDemoBlazor.Client.Components.Messaging
{
    public partial class MessageComponent : IDisposable
    {
        [Parameter]
        public MessageClass Message { get; set; } = null!;

        protected override void OnParametersSet()
        {
            if (Message != null)
            {
                Message.TimeUpdated += MessageUpdated;
            }
        }

        private void MessageUpdated(object? sender, EventArgs e)
        {
            StateHasChanged();
        }

        public void Dispose()
        {
            Message.TimeUpdated -= MessageUpdated;
            GC.SuppressFinalize(this);
        }
    }
}
