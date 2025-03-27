using System.Diagnostics;

namespace Ssera.Api.Infra.ExceptionHandling;

public static class StartupExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0058:Expression value is never used")]
    public static void AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = FixProblemDetailsTraces);

        services.AddExceptionHandler<ExceptionHandlerMiddleware>();
        services.AddSingleton<GlobalExceptionHandler>();

        RegisterProblemDetailsExceptionHandlers(services);
    }

    private static void FixProblemDetailsTraces(ProblemDetailsContext context)
    {
        // the problemdetailswriter unapologetically sets traceId to:
        // Activity.Current?.Id ?? httpContext.TraceIdentifier.
        // we fix that up here, its the only chance. we dont want it.
        // guarded by a check to make sure we do have the traces we want.
        // (if activityTraceId is not available, the traceId is the same as requestTraceId anyways)
        if (context.ProblemDetails.Extensions.ContainsKey("requestTraceId"))
        {
            _ = context.ProblemDetails.Extensions.Remove("traceId");
        }
    }

    private static void RegisterProblemDetailsExceptionHandlers(IServiceCollection services)
    {
        var interfaceName = typeof(IProblemDetailsExceptionHandler<>).FullName;
        Debug.Assert(interfaceName is not null);

        foreach (var (type, @interface) in typeof(StartupExtensions).Assembly.GetTypes()
            .Select(type => (Type: type, Interface: type.GetInterface(interfaceName)))
            .Where(t => t.Interface is not null))
        {
            Debug.Assert(@interface is not null);
            var exceptionType = @interface!.GetGenericArguments().Single();

            _ = services.AddSingleton(
                serviceType: typeof(IProblemDetailsExceptionHandler<>).MakeGenericType(exceptionType),
                implementationType: type
            );
        }
    }
}
