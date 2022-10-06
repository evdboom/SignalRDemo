using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Server.Services
{
    public class AnswerControl
    {
        public string GivenAnswer { get; }
        public DateTime AnswerDateTime { get; }
        public GameUser User { get; }

        public AnswerControl(string givenAnswer, GameUser user)
        {
            GivenAnswer = givenAnswer;
            User = user;
            AnswerDateTime = DateTime.UtcNow;
        }
    }
}
