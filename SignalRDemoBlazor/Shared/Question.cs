namespace SignalRDemoBlazor.Shared
{
    public class Question
    {
        public string Subject { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public DateTime AskTime { get; set; } = DateTime.MinValue;
        public int SecondsToAnswer { get; set; } = 10;
        public IList<string> Answers { get; set; } = new List<string>();
        public bool LastQuestion { get; set; }

    }
}
