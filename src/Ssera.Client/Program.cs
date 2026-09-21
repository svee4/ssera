using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Ssera.Client;
using Ssera.Client.Infra;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Radzen.ServiceCollectionExtensions.AddRadzenComponents(builder.Services);

var apiBaseAddress = builder.Configuration["SseraApiUrl"] is { Length: > 0 } apiUrl
    ? new Uri(apiUrl)
    : new Uri(builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
