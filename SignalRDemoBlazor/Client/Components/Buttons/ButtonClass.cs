using Microsoft.AspNetCore.Components.Web;

namespace SignalRDemoBlazor.Client.Components.Buttons
{
    public class ButtonClass
    {
        private const int _defaultPriority = 99;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Action<MouseEventArgs?>? ClickAction { get; set; }
        public Func<bool>? DisabledCondition { get; set; }
        public ActionType ActionType { get; set; }
        public ButtonTypes ButtonType { get; set; }
        public ButtonSize Size { get; set; }
        public bool IsSubmit { get; set; }
        public string OtherIconClass { get; set; } = string.Empty;
        public int Priority { get; set; }

        public ButtonClass()
        {
            ButtonType = ButtonTypes.Icon;
            ActionType = ActionType.Default;
            Size = ButtonSize.Normal;
            Priority = _defaultPriority;
        }

        public bool IsDisabled()
        {
            return DisabledCondition != null && DisabledCondition.Invoke();
        }

        public string GetButtonClass()
        {
            return $"{GetActionClass()} {GetSizeClass()}";
        }

        private string GetActionClass()
        {
            return ActionType switch
            {
                ActionType.Add => "btn btn-success",
                ActionType.Remove => "btn btn-danger",
                ActionType.Cancel => "btn btn-secondary",
                _ => "btn btn-primary"
            };
        }

        public string GetToolTip()
        {
            return Name;
        }

        private string GetSizeClass()
        {
            return Size switch
            {
                ButtonSize.ExtraSmall => "w50-inline",
                ButtonSize.Small => "w100-inline",
                ButtonSize.Normal => "w150-inline",
                ButtonSize.Large => "w200-inline",
                ButtonSize.ExtraLarge => "w250-inline",
                _ => "w150-inline"
            };
        }

        public string GetIcon()
        {
            return ActionType switch
            {
                ActionType.Add => "bi-plus",
                ActionType.Remove => "bi-trash",
                ActionType.Edit => "bi-pencil",
                ActionType.Search => "bi-search",
                ActionType.Refresh => "bi-arrow-repeat",
                ActionType.Cancel => "bi-x",
                ActionType.Confirm => "bi-check",
                ActionType.Other => OtherIconClass,
                _ => "bi-circle"
            };
        }

        public string GetButtonType()
        {
            return IsSubmit ? "submit" : "button";
        }
    }
}
