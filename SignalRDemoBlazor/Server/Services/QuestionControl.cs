using SignalRDemoBlazor.Shared;
using System.Collections.Concurrent;

namespace SignalRDemoBlazor.Server.Services
{
    public class QuestionControl
    {
        public bool Asked { get; set; }
        public Question Question { get; }
        public string CorrectAnswer { get; }
        public ConcurrentDictionary<string, AnswerControl> GivenAnswers { get; }

        public QuestionControl(Question question, string correctAnswer)
        {
            Question = question;
            CorrectAnswer = correctAnswer;

            if (!Question.Answers.Any(a => a == correctAnswer))
            {
                throw new ArgumentException($"{correctAnswer} is not a valid answer for the question: {question.QuestionText}");
            }
            GivenAnswers = new();
        }
    }
}
