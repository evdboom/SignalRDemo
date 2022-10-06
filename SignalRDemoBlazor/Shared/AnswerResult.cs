namespace SignalRDemoBlazor.Shared
{
    public class AnswerResult
    {
        public string CorrectAnswer { get; set; } = string.Empty;
        public GameUser? Quickest { get; set; }
        public List<Score> Scores { get; set; } = new();
    }
}
