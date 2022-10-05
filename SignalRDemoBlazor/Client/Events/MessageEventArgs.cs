using SignalRDemoBlazor.Client.Components.Messaging;

namespace SignalRDemoBlazor.Client.Events
{
    public class MessageEventArgs
    {
        public MessageClass Message { get; }

        public MessageEventArgs(MessageClass message)
        {
            Message = message;
        }
    }
}
