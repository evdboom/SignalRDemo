using Microsoft.AspNetCore.Components;
using SignalRDemoBlazor.Client.Events;

namespace SignalRDemoBlazor.Client.Components.Messaging
{
    public partial class MessageContainer : IDisposable
    {
        [Inject]
        private MessageService MessageService { get; set; } = null!;

        protected override void OnInitialized()
        {
            MessageService.MessageListChanged += MessagesChanged;
        }

        private void MessagesChanged(object? sender, MessageEventArgs e)
        {
            StateHasChanged();
        }

        public void Dispose()
        {
            MessageService.MessageListChanged -= MessagesChanged;
            GC.SuppressFinalize(this);
        }
    }
}
