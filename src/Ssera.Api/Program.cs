using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Ssera.Api;
using Ssera.Api.Database;
using Ssera.Api.Infra.Configuration;
using Ssera.Api.Infra.ProblemDetailsHandler;
using Ssera.Api.Infra.Startup;
using Ssera.Api.Worker;

[assembly: Behaviors(typeof(ValidationBehavior<,>))]

var builder = WebApplication.CreateBuilder(args);

var sqliteConnectionString = builder.Configuration.GetRequiredValue("ConnectionStrings:Sqlite");

builder.Services.AddSqlite<AppDbContext>(sqliteConnectionString);

builder.Services.AddHandlers();
builder.Services.AddBehaviors();

builder.Services.AddProblemDetailsHandler();
builder.Services.AddCors();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSwagger();

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
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();
