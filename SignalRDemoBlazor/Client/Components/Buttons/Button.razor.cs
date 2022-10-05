using Microsoft.AspNetCore.Components;

namespace SignalRDemoBlazor.Client.Components.Buttons
{
    public partial class Button
    {
        [Parameter]
        public ButtonClass Item { get; set; } = null!;
    }
}
