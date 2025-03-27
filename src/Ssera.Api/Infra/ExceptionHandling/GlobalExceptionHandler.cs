using Microsoft.AspNetCore.Mvc;

namespace Ssera.Api.Infra.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger)
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly bool _isDevEnv = environment.IsDevelopment();

    public Task<ProblemDetails> Handle(
        HttpContext context,
        Exception exception,
        CancellationToken token)
    {
        _logger.LogError("Unhandled exception for '{TraceId}':\n{Exception}", context.TraceIdentifier, exception);
        return Task.FromResult(new ProblemDetails
        {
            Status = 500,
            Title = "Unhandled server error",
            Detail = _isDevEnv ? exception.ToString() : "Unhandled exception"
        });
    }
}
