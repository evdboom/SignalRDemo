using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Events
{
    public class QuestionEventArgs
    {
        public Question Question { get; }

        public QuestionEventArgs(Question question)
        {
            Question = question;
        }
    }
}
