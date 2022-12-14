using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SignalRDemoBlazor.Client;
using SignalRDemoBlazor.Client.Components.Messaging;
using SignalRDemoBlazor.Client.Services;
using SignalRDemoBlazor.Client.Services.Storage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services
    .AddSingleton<LocalStorageService>()
    .AddSingleton<SessionStorageService>()
    .AddSingleton<SignalRService>()
    .AddSingleton<MessageService>()
    .AddSingleton<ScrollService>();

await builder.Build().RunAsync();
