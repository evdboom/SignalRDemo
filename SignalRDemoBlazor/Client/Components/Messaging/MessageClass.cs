using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Components.Messaging
{
    public class MessageClass : IDisposable
    {
        public const int DefaultOpenTime = 5;

        public AlertType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool ShowRemainingTime { get; set; }
        public GameUser? User { get; set; }

        public string? Code { get; set; }

        /// <summary>
        /// Gets or Sets the time to display the message in seconds, changing this value will not influence an open message.
        /// </summary>
        public int OpenTime { get; set; }
        public bool IsOpen => _isOpen;

        private bool _isOpen;
        private Timer? _timer;
        private int _timeRemaining;

        public event EventHandler? ShouldClose;
        public event EventHandler? TimeUpdated;

        public MessageClass()
        {
            OpenTime = DefaultOpenTime;
            ShowRemainingTime = true;
        }

        public string GetAlertType()
        {
            return Type switch
            {
                AlertType.Information => "alert alert-info",
                AlertType.Warning => "alert alert-warning",
                AlertType.Danger => "alert alert-danger",
                AlertType.Success => "alert alert-success",
                _ => "alert alert-primary"
            };
        }

        public int GetRemainingSeconds()
        {
            return _timeRemaining;
        }

        public void Open()
        {
            if (_isOpen)
            {
                return;
            }
            _isOpen = true;
            _timeRemaining = OpenTime;

            if (_timeRemaining > 0)
            {
                _timer = new Timer(Elapsed, null, 0, 1000);
            }
            else
            {
                ShowRemainingTime = false;
            }
        }

        public void Close()
        {
            _timeRemaining = 0;
            Elapsed(null);
        }

        private void Elapsed(object? state)
        {
            _timeRemaining--;

            if (_timeRemaining > 0)
            {
                TimeUpdated?.Invoke(this, new EventArgs());
            }
            else
            {
                _isOpen = false;
                ShouldClose?.Invoke(this, new EventArgs());
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
