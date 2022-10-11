using Microsoft.AspNetCore.SignalR;
using SignalRDemoBlazor.Server.Hubs;
using SignalRDemoBlazor.Shared;
using System.Collections.Concurrent;

namespace SignalRDemoBlazor.Server.Services
{
    public class GameManager : IGameManager
    {

        private readonly IHubContext<DemoHub> _hubContext;
        private readonly List<QuestionControl> _questions;
        private QuestionControl? _currentQuestion;
        private readonly List<GameUser> _users;
        private readonly List<GameUser> _removedUsers;
        private readonly ConcurrentDictionary<string, Score> _scores;
        private readonly string[] _groups;
        private const string PinCode = "918273";
        private int[] _scoreList;

        private int _lastGroup;
        private bool _mayEnable;
        public bool MayEnable => _mayEnable;

        public GameManager(IHubContext<DemoHub> hubContext)
        {
            _hubContext = hubContext;
            _removedUsers = new();
            _users = new();
            _groups = new[]
            {
                "red",
                "green",
                "blue"
            };
            _scores = new ConcurrentDictionary<string, Score>(_groups
                .ToDictionary(g => g, g => new Score
                {
                    Group = g
                }));
            _scoreList = new[]
            {
                10,
                9,
                7,
                6,
                4,
                3,
                2,
                1
            };
            _lastGroup = -1;
            _questions = GetQuestions()
                        .ToList();
        }

        public void Enable()
        {
            _mayEnable = true;
        }

        private IEnumerable<QuestionControl> GetQuestions()
        {
            yield return GetQuestion1();
            yield return GetQuestion2();
            yield return GetQuestion3();
        }

        private QuestionControl GetQuestion1()
        {
            var question = new Question
            {
                Subject = "Technologieën",
                QuestionText = "Als welk protocol ondersteund wordt geeft SignalR het beste resultaat?",
                Answers = new List<string>
                {
                    "Server sent events",
                    "Websockets",
                    "HTTP POST",
                    "Long polling"
                }
            };
            var answer = "Websockets";
            return new QuestionControl(question, answer);
        }

        private QuestionControl GetQuestion2()
        {
            var question = new Question
            {
                Subject = "Toepassing",
                QuestionText = "Op welke van deze use cases is het gebruik van SignalR van geen/weinig toegevoegde waarde?",
                Answers = new List<string>
                {
                    "Verwerking upload",
                    "Comments sectie",
                    "Blog",
                    "Geen (altijd goed)"
                }
            };
            var answer = "Blog";
            return new QuestionControl(question, answer);
        }

        private QuestionControl GetQuestion3()
        {
            var question = new Question
            {
                Subject = "Verhouding",
                QuestionText = "SignalR staat tot website als ... staat tot auto ?",
                Answers = new List<string>
                {
                    "Carosserie",
                    "Parkeersensoren",
                    "Stuur"
                },
                LastQuestion = true,
                SecondsToAnswer = 15
            };
            var answer = "Parkeersensoren";
            return new QuestionControl(question, answer);
        }

        public List<GameUser> GetUsers(string excludeConnectionId)
        {
            return _users
                .Where(user => user.ConnectionId != excludeConnectionId)
                .ToList();
        }

        public bool CanRegister(string userName, string pinCode, string connectionId, out string message)
        {
            if (pinCode != PinCode)
            {
                message = $"Incorrect PIN code";
            }
            else if (_users.Any(user => user.Name.ToLower() == userName.ToLower()))
            {
                message = $"Username {userName} is already taken.";
            }
            else if (_users.Any(user => user.ConnectionId == connectionId))
            {
                message = $"You are already connected, reload page in a new tab to force a new connection";
            }
            else
            {
                message = string.Empty;
            }

            return string.IsNullOrEmpty(message);
        }

        public GameUser? FindUser(string userName)
        {
            return _users.SingleOrDefault(user => user.Name.ToLower() == userName.ToLower());
        }

        public GameUser? FindUserByConnection(string connectionId)
        {
            return _users.SingleOrDefault(user => user.ConnectionId == connectionId);

        }

        public GameUser? ReregisterUser(string userName, string connectionId)
        {
            if (_removedUsers.FirstOrDefault(user => user.Name == userName) is GameUser reconnect && FindUser(userName) is null)
            {
                reconnect.ConnectionId = connectionId;
                _users.Add(reconnect);
                _removedUsers.Remove(reconnect);

                return reconnect;
            }

            return null;
        }

        public GameUser RegisterUser(string userName, string connectionId)
        {
            if (!CanRegister(userName, PinCode, connectionId, out string message))
            {
                throw new InvalidOperationException(message);
            }

            var user = new GameUser
            {
                ConnectionId = connectionId,
                Group = GetNextGroup(),
                Name = userName
            };
            _users.Add(user);

            return user;
        }

        public GameUser? RemoveUser(string connectionId)
        {
            var user = FindUserByConnection(connectionId);
            if (user is not null)
            {
                _users.Remove(user);
                _removedUsers.Add(user);
            }

            return user;
        }

        private string GetNextGroup()
        {
            _lastGroup++;
            if (_lastGroup >= _groups.Length)
            {
                _lastGroup = 0;
            }
            return _groups[_lastGroup];
        }

        public void AnswerQuestion(GameUser sender, string question, string answer)
        {
            if (_currentQuestion is null || _currentQuestion.Question.QuestionText != question)
            {
                return;
            }

            _currentQuestion.GivenAnswers[sender.Name] = new AnswerControl(answer, sender);
        }

        public Question? GetCurrentQuestion()
        {
            return _currentQuestion?.Question;
        }

        public void StartQuestion()
        {
            if (_currentQuestion is null)
            {
                return;
            }

            _currentQuestion.Question.AskTime = DateTime.UtcNow;
            _ = new Timer(CompleteQuestion, null, _currentQuestion.Question.SecondsToAnswer * 1000, Timeout.Infinite);            
        }

        private async void CompleteQuestion(object? state)
        {
            if (_currentQuestion is null)
            {
                return;
            }

            var question = _currentQuestion;
            _currentQuestion = null;
            var scores = CalculateScores(question, out GameUser? quickest);
            await _hubContext
                .Clients
                .All
                .SendAsync(MessageType.QuestionDone, new AnswerResult
                {
                    CorrectAnswer = question.CorrectAnswer,
                    Quickest = quickest,
                    Scores = scores
                });
        }

        public Question? GetNextQuestion()
        {
            var question = _questions
                .FirstOrDefault(q => !q.Asked);

            if (question is null)
            {
                return null;
            }

            question.Asked = true;
            _currentQuestion = question;

            return question.Question;
        }

        private List<Score> CalculateScores(QuestionControl question, out GameUser? quickest)
        {
            quickest = null;

            var correct = question.GivenAnswers.Values
                .Where(a => a.GivenAnswer == question.CorrectAnswer)
                .OrderBy(a => a.AnswerDateTime)
                .ToArray();

            for (int i = 0; i < correct.Length; i++)
            {
                var answer = correct[i];
                _scores[answer.User.Group].TotalScore += _scoreList[i];
                quickest ??= answer.User;
            }

            return _scores
                .Select(s => s.Value)
                .OrderByDescending(s => s.TotalScore)
                .ToList();
        }
    }
}
