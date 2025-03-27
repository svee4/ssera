using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Ssera.Api.Infra.ExceptionHandling;

public sealed class ExceptionHandlerMiddleware(
    IServiceProvider serviceProvider,
    GlobalExceptionHandler globalHandler,
    IProblemDetailsService problemDetailsService,
    ILogger<ExceptionHandlerMiddleware> logger) : IExceptionHandler
{
    private static readonly MethodInfo _handlerMethod = typeof(ExceptionHandlerMiddleware)
        .GetMethod($"{nameof(InvokeHandler)}", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Method Handle not found");

    private static readonly ConcurrentDictionary<Type, MethodInfo> _typedHandlers = [];

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly GlobalExceptionHandler _globalHandler = globalHandler;
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

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

        var handler = _typedHandlers.GetOrAdd(
            exception.GetType(),
            (type, method) => method.MakeGenericMethod(type),
            _handlerMethod);

        var problem = await (Task<ProblemDetails>)handler.Invoke(this, [httpContext, exception, cancellationToken])!;

        if (problem.Status is not { } statusCode)
        {
            throw new InvalidOperationException("Exception handler left ProblemDetails in invalid state (no status)");
        }

        // we want the extensions to contain both "requestTraceId" and "activityTraceId",
        // as they are two distinct traces.
        // the problem details service writer unapologetically sets "traceId",
        // but we fix that up in StartupExtensions.FixProblemDetailsTraces

        if (Activity.Current?.Id is { } activityId)
        {
            _ = problem.Extensions.TryAdd("activityTraceId", activityId);
        }

        _ = problem.Extensions.TryAdd("requestTraceId", httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;

        var details = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        };

        await _problemDetailsService.WriteAsync(details);
        return true;
    }

    private async Task<ProblemDetails> InvokeHandler<TException>(
        HttpContext context,
        TException exception,
        CancellationToken token) where TException : Exception
    {
        var handler = _serviceProvider.GetService<IProblemDetailsExceptionHandler<TException>>();
        var task = handler is not null
            ? handler.Handle(context, exception, token)
            : _globalHandler.Handle(context, exception, token);

        return await task;
    }
}
