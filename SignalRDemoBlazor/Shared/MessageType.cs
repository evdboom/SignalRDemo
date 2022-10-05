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
        public const string UserListReceived = "UserListReceived";

    }
}
