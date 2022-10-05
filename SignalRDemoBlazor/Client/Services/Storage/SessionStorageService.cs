using Microsoft.JSInterop;

namespace SignalRDemoBlazor.Client.Services.Storage
{
    public class SessionStorageService : StorageServiceBase
    {
        private const string _storageLocation = "sessionStorage";

        public SessionStorageService(IJSRuntime jsRuntime) : base(jsRuntime, _storageLocation)
        {
        }
    }
}
