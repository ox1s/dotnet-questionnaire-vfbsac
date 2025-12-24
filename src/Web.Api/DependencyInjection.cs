using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Web.Api.Infrastructure;

namespace Web.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        services.Configure<JsonOptions>(options =>
                {
                    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
