using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;

namespace SignalRDemoBlazor.Client.Services.Storage
{
    public abstract class StorageServiceBase
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _storageLocation;

        public StorageServiceBase(IJSRuntime jsRuntime, string storageLocation)
        {
            _jsRuntime = jsRuntime;
            _storageLocation = storageLocation;
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);

            await _jsRuntime.InvokeVoidAsync($"{_storageLocation}.setItem", key, json);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync($"{_storageLocation}.removeItem", key);
        }

        public async Task<T?> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string>($"{_storageLocation}.getItem", key);

            if (json == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task SetItemIfEmptyAsync<T>(string key, T value)
        {
            var current = await GetItemAsync<T>(key);
            if (current == null)
            {
                await SetItemAsync(key, value);
            }
        }
    }
}
