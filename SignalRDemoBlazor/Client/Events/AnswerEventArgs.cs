using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Events
{
    public class AnswerEventArgs
    {
        public AnswerResult Result { get; }

        public AnswerEventArgs(AnswerResult result)
        {
            Result = result;
        }
    }
}
