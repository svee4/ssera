using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Ssera.Api;
using Ssera.Api.Data;
using Ssera.Api.Infra.Configuration;
using Ssera.Api.Infra.ExceptionHandling;
using Ssera.Api.Ingestion.EventArchive;
using Ssera.Api.Ingestion.EventArchive.Mappers;
using Ssera.Api.Ingestion.ImageArchive;

[assembly: Behaviors(typeof(ValidationBehavior<,>))]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<ApiDbContext>(builder.Configuration.GetRequiredValue("ConnectionStrings:Sqlite"));

builder.Services.AddSseraApiHandlers();
builder.Services.AddSseraApiBehaviors();

builder.Services.AddExceptionHandling();
builder.Services.AddOpenApi();
builder.Services.AddCors();

// throw minimal api model binding exceptions in development too
// BECAUSE OTHERWISE THERES NO FUCKING WAY TO KNOW SOMETHING WENT WRONG
// AND RETURN A STRUCTURED RESPONSE
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

builder.Services.AddHostedService<EventArchiveWorker>();
builder.Services.AddHostedService<ImageArchiveWorker>();

builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKind.TeasersMV.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, PerformanceVarietyRealityMapper>(EventArchiveEventKind.Performance.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, MusicShowsMapper>(EventArchiveEventKind.MusicShows.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKind.BehindTheScenes.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKind.Interview.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, PerformanceVarietyRealityMapper>(EventArchiveEventKind.Variety.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, PerformanceVarietyRealityMapper>(EventArchiveEventKind.Reality.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, CFMapper>(EventArchiveEventKind.CF.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKind.Misc.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKind.MubankPresident.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, WeverseMapper>(EventArchiveEventKind.WeverseLive.AsHuman());

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
    _ = app.MapScalarApiReference();
    _ = app.UseCors(options => options.AllowAnyOrigin());
}
else
{
    var options = new ForwardedHeadersOptions()
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        ForwardLimit = 1
    };

    // allow all networks because the app is ran exclusively behind a single trusted proxy
    // and ForwardLimit is 1
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    _ = app.UseForwardedHeaders(options);

    var cors = app.Configuration.GetRequiredValue("CorsAllowOrigins").Split(",");
    logger.LogInformation("Using cors: {AllowedOrigins}", cors);
    _ = app.UseCors(options => options.WithOrigins(cors));
}

app.UseHttpsRedirection();
app.MapSseraApiEndpoints();

app.MapGet("/", () => TypedResults.Redirect("/scalar/v1"));

await using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    //_ = await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();

partial class Program;
