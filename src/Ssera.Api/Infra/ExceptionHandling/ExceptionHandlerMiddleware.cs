using Immediate.Validations.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Ssera.Api.Infra.ExceptionHandling;

public sealed class ExceptionHandlerMiddleware(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<ExceptionHandlerMiddleware> logger) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly IHostEnvironment _environment = environment;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

    private bool IsDevEnv => _environment.IsDevelopment();

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            return await HandleCore(httpContext, exception, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Uncaught exception in exception handler: {Exception}", ex);
            return false;
        }
    }

    private async ValueTask<bool> HandleCore(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling {Exception}", exception);

        ProblemDetails details;

        if (exception is ValidationException validationException)
        {
            details = new ValidationProblemDetails
            {
                Status = 400,
                Title = "Validation problem occurred",
                Detail = validationException.Message,
                Errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.ErrorMessage).ToArray())
            };
        }
        else if (exception is BadHttpRequestException httpRequestException)
        {
            details = new ValidationProblemDetails
            {
                Status = 400,
                Title = "Bad request",
                Detail = httpRequestException.InnerException?.Message ?? httpRequestException.Message
            };
        }
        else
        {
            _logger.LogError("Unhandled exception for '{TraceId}': {Exception}", Activity.Current?.Id, exception);

            details = new ProblemDetails
            {
                Status = 500,
                Title = "Unhandled server error",
                Detail = IsDevEnv ? exception.ToString() : "Unhandled exception"
            };
        }

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = details,
        };

        httpContext.Response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;
        await _problemDetailsService.WriteAsync(context);
        return true;
    }
}
