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
using Ssera.Api.Ingestion.EventArchive.Mappers;

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

builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKid.TeasersMV.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, PerformanceVarietyRealityMapper>(EventArchiveEventKid.Performance.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, MusicShowsMapper>(EventArchiveEventKid.MusicShows.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKid.BehindTheScenes.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKid.Interview.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, PerformanceVarietyRealityMapper>(EventArchiveEventKid.Variety.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, PerformanceVarietyRealityMapper>(EventArchiveEventKid.Reality.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, CFMapper>(EventArchiveEventKid.CF.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKid.Misc.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, DefaultMapper>(EventArchiveEventKid.MubankPresident.AsHuman());
builder.Services.AddKeyedScoped<IEventArchiveSheetMapper, WeverseMapper>(EventArchiveEventKid.WeverseLive.AsHuman());

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
