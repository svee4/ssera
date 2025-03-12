using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Ssera.Api;
using Ssera.Api.Data;
using Ssera.Api.Infra.Configuration;
using Ssera.Api.Infra.ProblemDetailsHandler;
using Ssera.Api.Infra.Startup;
using Ssera.Api.Ingestion;
using Ssera.Api.Ingestion.Archive.Mappers;

[assembly: Behaviors(typeof(ValidationBehavior<,>))]

var builder = WebApplication.CreateBuilder(args);

var sqliteConnectionString = builder.Configuration.GetRequiredValue("ConnectionStrings:Sqlite");

builder.Services.AddSqlite<ApiDbContext>(sqliteConnectionString);

builder.Services.AddSseraApiHandlers();
builder.Services.AddSseraApiBehaviors();

builder.Services.AddProblemDetailsHandler();
builder.Services.AddCors();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSwagger();

builder.Services.AddKeyedScoped<IEventSheetMapper, DefaultMapper>(EventSheetEventKind.TeasersMV.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, PerformanceVarietyRealityMapper>(EventSheetEventKind.Performance.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, MusicShowsMapper>(EventSheetEventKind.MusicShows.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, DefaultMapper>(EventSheetEventKind.BehindTheScenes.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, DefaultMapper>(EventSheetEventKind.Interview.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, PerformanceVarietyRealityMapper>(EventSheetEventKind.Variety.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, PerformanceVarietyRealityMapper>(EventSheetEventKind.Reality.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, CFMapper>(EventSheetEventKind.CF.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, DefaultMapper>(EventSheetEventKind.Misc.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, DefaultMapper>(EventSheetEventKind.MubankPresident.AsHuman());
builder.Services.AddKeyedScoped<IEventSheetMapper, WeverseMapper>(EventSheetEventKind.WeverseLive.AsHuman());

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    _ = app.UseCors(options => options.AllowAnyOrigin());
}
else
{
    _ = app.UseForwardedHeaders();
    _ = app.UseCors(options => options.WithOrigins(app.Configuration.GetRequiredValue("CorsAllowOrigins").Split(",")));
}

app.UseHttpsRedirection();

app.MapSseraApiEndpoints();

await using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();
