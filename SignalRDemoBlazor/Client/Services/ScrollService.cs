using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SignalRDemoBlazor.Client.Services
{
    public class ScrollService
    {        
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

        public ScrollService(IJSRuntime jSRuntime)
        {
            _moduleTask = new(() => jSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/scroll-service.js").AsTask());
        }

        public async Task ScrollToBottom(ElementReference element)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("scrollToBottom", element);
        }
    }
}
