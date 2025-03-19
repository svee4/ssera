using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Ssera.Api;
using Ssera.Api.Data;
using Ssera.Api.Infra.Configuration;
using Ssera.Api.Infra.ProblemDetailsHandler;
using Ssera.Api.Infra.Startup;
using Ssera.Api.Ingestion.EventArchive;
using Ssera.Api.Ingestion.EventArchive.Mappers;
using Ssera.Api.Ingestion.ImageArchive;

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

//builder.Services.AddHostedService<EventArchiveWorker>();
builder.Services.AddHostedService<ImageArchiveWorker>();

builder.Services.AddSwagger();

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
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();
