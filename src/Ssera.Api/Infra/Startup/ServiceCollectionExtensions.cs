using Microsoft.AspNetCore.Builder;

namespace Ssera.Api.Infra.Startup;

public static class ServiceCollectionExtensions
{
    public static void AddSwagger(this IServiceCollection services)
    {
        _ = services.AddEndpointsApiExplorer();
        _ = services.AddSwaggerGen(options => options.CustomSchemaIds(x => x.FullName?.Replace("+", ".", StringComparison.Ordinal)));
    }

    public static void UseSwagger(this IApplicationBuilder app)
    {
        _ = SwaggerBuilderExtensions.UseSwagger(app);
        _ = app.UseSwaggerUI(options =>
        {
            // serve swagger at root
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            options.RoutePrefix = "";
        });
    }
}
