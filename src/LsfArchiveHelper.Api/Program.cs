using System.Text.Json.Serialization;
using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using LsfArchiveHelper.Api;
using LsfArchiveHelper.Api.Database;
using LsfArchiveHelper.Api.Infra.ProblemDetailsHandler;
using LsfArchiveHelper.Api.Worker;

[assembly: Behaviors(typeof(ValidationBehavior<,>))] 

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: false);

var sqliteConnectionString = builder.Configuration["ConnectionStrings:Sqlite"] ??
                             throw new InvalidOperationException("Sqlite connection string not configured");

builder.Services.AddSqlite<AppDbContext>(sqliteConnectionString);

builder.Services.AddHandlers();
builder.Services.AddBehaviors();
builder.Services.AddProblemDetailsHandler();

builder.Services.AddHostedService<Worker>();

builder.Services.AddCors();

builder.Services.AddSwagger();

// builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => 
// 	opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapLsfArchiveHelperApiEndpoints();

app.Run();
