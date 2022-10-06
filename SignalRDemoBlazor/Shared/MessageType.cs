namespace SignalRDemoBlazor.Shared
{
    public static class MessageType
    {
        public const string MessageReceived = "MessageReceived";
        public const string ChatReceived = "ChatReceived";
        public const string QuestionReceived = "QuestionReceived";

        public const string SendMessage = "SendMessage";
        public const string SendChat = "ChatMessage";

        public const string RegisterUser = "RegisterUser";
        public const string ReregisterUser = "ReregisterUser";

        public const string UserJoined = "UserJoined";
        public const string UserLeft = "UserLeft";
        public const string GetUserList = "GetUserList";

        public const string AnswerQuestion = "AnswerQuestion";
        public const string QuestionProgress = "QuestionProgress";
        public const string QuestionDone = "QuestionDone";

        public const string StartQuiz = "StartQuiz";
        public const string StartQuestion = "StartQuestion";

        public const string QuestionStarted = "QuestionStarted";
        public const string GetState = "GetState";
    }
}
