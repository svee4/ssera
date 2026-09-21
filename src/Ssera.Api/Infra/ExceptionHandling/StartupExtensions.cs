namespace Ssera.Api.Infra.ExceptionHandling;

public static class StartupExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0058:Expression value is never used")]
    public static void AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ExceptionHandlerMiddleware>();
   }
}
