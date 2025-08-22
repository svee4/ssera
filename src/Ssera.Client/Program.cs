using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Ssera.Client;
using Ssera.Client.Infra;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Radzen.ServiceCollectionExtensions.AddRadzenComponents(builder.Services);

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ThemeService>();

//builder.Services.AddScoped(sp
//    => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
