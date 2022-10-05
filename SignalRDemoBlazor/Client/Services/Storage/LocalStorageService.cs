using Microsoft.JSInterop;

namespace SignalRDemoBlazor.Client.Services.Storage
{
    public class LocalStorageService : StorageServiceBase
    {
        private const string _storageLocation = "localStorage";

        public LocalStorageService(IJSRuntime jsRuntime) : base(jsRuntime, _storageLocation)
        {
        }
    }
}
