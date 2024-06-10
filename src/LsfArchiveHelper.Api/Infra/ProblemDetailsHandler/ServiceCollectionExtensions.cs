namespace LsfArchiveHelper.Api.Infra.ProblemDetailsHandler;

public static class ServiceCollectionExtensions
{
	public static void AddProblemDetailsHandler(this IServiceCollection services) =>
		services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsHandler.Handle);
}
