using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SignalRDemoBlazor.Client.Components.Buttons;
using SignalRDemoBlazor.Client.Components.Modals;
using SignalRDemoBlazor.Client.Events;
using SignalRDemoBlazor.Client.Services;
using SignalRDemoBlazor.Shared;

namespace SignalRDemoBlazor.Client.Components.Game
{
    public partial class GameMain
    {
        [Parameter]
        public string Group { get; set; } = string.Empty;
        [Parameter]
        public string UserName { get; set; } = string.Empty;

        [Inject]
        private SignalRService SignalRService { get; set; } = null!;

        private MessageWindow MessageWindow { get; set; } = null!;
        private Modal AnswerModal { get; set; } = null!;
        private Modal QuestionModal { get; set; } = null!;
        private ModalSize ModalSize { get; set; }
        private ButtonClass RefreshButton { get; set; } = null!;
        private ButtonClass StartQuizButton { get; set; } = null!;
        private ButtonClass StartQuestionButton { get; set; } = null!;
        private ButtonClass ActivateSignalRButton { get; set; } = null!;
        private Question? CurrentQuestion { get; set; }
        private List<ButtonClass> CurrentAnswerOptions { get; set; } = new();
        private AnswerResult? Result { get; set; }
        private string CurrentAnswer { get; set; } = string.Empty;
        private int CurrentQuestionProgress { get; set; }
        private int CurrentQuestionMaxProgress { get; set; }
        private bool CurrentQuestionStarted { get; set; }
        private bool LastQuestion { get; set; }
        private bool MayEnable { get; set; }

        protected override async Task OnInitializedAsync()
        {
            RefreshButton = new()
            {
                ActionType = ActionType.Refresh,
                ButtonType = ButtonTypes.Full,
                Name = "Refresh",
                ClickAction = async args => await Refresh()
            };
            StartQuizButton = new()
            {
                ButtonType = ButtonTypes.Name,
                Name = "Start quiz",
                ClickAction = async args => await SignalRService.StartQuiz(),
                DisabledCondition = () => UserName != "Erik" //Very secure :).
            };
            StartQuestionButton = new()
            {
                ButtonType = ButtonTypes.Name,
                Name = "Start",
                ClickAction = async args => await SignalRService.StartQuestion(),
                DisabledCondition = () => UserName != "Erik" // Very secure :).
            };
            ActivateSignalRButton = new()
            {
                ButtonType = ButtonTypes.Name,
                Name = "Activate",
                ClickAction = async args => await ActivateSignalR(),
                DisabledCondition = () => UserName != "Erik" // Very secure :).
            };

            SignalRService.QuestionAsked += QuestionAsked;
            SignalRService.QuestionProgressUpdated += UpdateProgress;
            SignalRService.QuestionDone += QuestionDone;
            SignalRService.QuestionStarted += QuestionStarted;
            SignalRService.MayEnable += SetMayEnable;

            MayEnable = await SignalRService.GetMayEnable();

        }

        private async Task ActivateSignalR()
        {
            await SignalRService.ActivateSignalR();
        }

        private void SetMayEnable(object? sender, bool mayEnable)
        {
            MayEnable = mayEnable;
            StateHasChanged();
        }

        private void QuestionStarted(object? sender, EventArgs e)
        {
            CurrentQuestionStarted = true;
            StateHasChanged();
        }

        private async void QuestionDone(object? sender, AnswerEventArgs e)
        {
            if (QuestionModal is not null)
            {
                await QuestionModal.Close();
            }
            CurrentQuestion = null;
            CurrentQuestionMaxProgress = 100;
            CurrentQuestionProgress = 0;
            CurrentQuestionStarted = false;
            Result = e.Result;
            StateHasChanged();
            if (AnswerModal is not null)
            {
                await AnswerModal.Show();
            }
        }

        private void UpdateProgress(object? sender, ProgressEventArgs e)
        {
            CurrentQuestionProgress = (int)e.Loaded;
            CurrentQuestionMaxProgress = (int)e.Total;
            StateHasChanged();
        }

        private async void QuestionAsked(object? sender, QuestionEventArgs e)
        {
            if (AnswerModal is not null)
            {
                await AnswerModal.Close();
            }

            Result = null;
            CurrentQuestion = e.Question;
            CurrentQuestionProgress = 0;
            CurrentQuestionMaxProgress = 100;
            CurrentAnswerOptions = GetCurrentAnswerOptions(CurrentQuestion)
                    .ToList();
            LastQuestion = e.Question.LastQuestion;
            StateHasChanged();
            if (QuestionModal is not null)
            {
                await QuestionModal.Show();
            }
            
        }

        private async Task Refresh()
        {
            await SignalRService.Refresh();
            await MessageWindow.Refresh();
        }

        private string GetStyle()
        {
            if (string.IsNullOrEmpty(Group))
            {
                return string.Empty;
            }

            return $"color: {Group}";
        }

        private async Task Enable()
        {
            await Refresh();
            SignalRService.Enable();
        }

        private void Disable()
        {
            SignalRService.Disable();
        }

        private IEnumerable<ButtonClass> GetCurrentAnswerOptions(Question? question)
        {
            if (question is null)
            {
                yield break;
            }

            foreach(var answer in question.Answers)
            {
                yield return new ButtonClass
                {
                    ButtonType = ButtonTypes.Full,
                    ClickAction = async args => await AnswerQuestion(answer),
                    Name = answer,
                    Size = ButtonSize.Large
                };
            }
        }

        private bool IsWinner(Score score)
        {
            if (!LastQuestion || Result is null)
            {
                return false;
            }

            return Result
                .Scores
                .OrderByDescending(s => s.TotalScore)
                .First().Group == score.Group;
        }

        private async Task AnswerQuestion(string answer)
        {
            foreach(var button in CurrentAnswerOptions)
            {
                button.IsSelected = button.Name == answer;
            }

            StateHasChanged();

            await SignalRService.AnswerQuestion(CurrentQuestion!.QuestionText, answer);            
        }
    }
}
