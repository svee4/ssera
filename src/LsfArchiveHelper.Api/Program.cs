using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using LsfArchiveHelper.Api;
using LsfArchiveHelper.Api.Database;
using LsfArchiveHelper.Api.Infra.Configuration;
using LsfArchiveHelper.Api.Infra.ProblemDetailsHandler;
using LsfArchiveHelper.Api.Worker;
using LsfArchiveHelper.Api.Infra.Startup;
using Microsoft.AspNetCore.HttpOverrides;

[assembly: Behaviors(typeof(ValidationBehavior<,>))] 

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: false);

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
	app.UseCors(options => options.AllowAnyOrigin());
}
else
{
	app.UseForwardedHeaders();
	app.UseHsts();
	app.UseCors(options => options.WithOrigins(app.Configuration.GetRequiredValue("CorsAllowOrigin")));
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapLsfArchiveHelperApiEndpoints();

app.Run();
