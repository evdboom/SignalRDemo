using SignalRDemoBlazor.Client.Events;
using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Components.Messaging
{
    public class MessageService : IDisposable
    {
        private readonly List<MessageClass> _messages;

        public event EventHandler<MessageEventArgs>? MessageListChanged;

        public MessageService()
        {
            _messages = new();
        }

        /// <summary>
        /// Display a message with the given title body and style for the given duration (in seconds)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <param name="type"></param>
        /// <param name="openTime"></param>
        public void DisplayMessage(string title, string body, AlertType type, int openTime)
        {
            var message = new MessageClass
            {
                Body = body,
                Title = title,
                Type = type,
                OpenTime = openTime
            };
            message.ShouldClose += RemoveMessage;
            message.Open();
            _messages.Add(message);
            MessageListChanged?.Invoke(this, new MessageEventArgs(message));


        }

        /// <summary>
        /// Display a message with the given title body and style for the default duration
        /// </summary>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <param name="type"></param>
        public void DisplayMessage(string title, string body, AlertType type)
        {
            DisplayMessage(title, body, type, MessageClass.DefaultOpenTime);
        }

        public IEnumerable<MessageClass> GetOpenMessages()
        {
            return _messages
                .Where(m => m.IsOpen);
        }

        public void Dispose()
        {
            foreach (var message in _messages)
            {
                message.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private void RemoveMessage(object? sender, EventArgs e)
        {
            if (sender is not MessageClass message)
            {
                return;
            }

            _messages.Remove(message);
            MessageListChanged?.Invoke(this, new MessageEventArgs(message));
            (message).Dispose();
        }
    }
}
