using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Ssera.Client;
using Ssera.Client.Infra;
using Ssera.Shared.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Radzen.ServiceCollectionExtensions.AddRadzenComponents(builder.Services);

var apiUrl = new Uri(builder.Configuration.GetRequiredValue("SseraApiUrl"));

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ThemeService>();

builder.Services.AddScoped<ImageApiService>();

builder.Services.AddHttpClient<ImageApiService, ImageApiService>(options =>
{
    options.BaseAddress = new Uri(apiUrl, "api/images");
});


//builder.Services.AddScoped(sp
//    => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
