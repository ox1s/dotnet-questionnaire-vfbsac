using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Web.Api.Infrastructure;

namespace Web.Api;

public static class DependencyInjection
{
    public const string CorsPolicyName = "Frontend";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddControllers();

        services.Configure<JsonOptions>(options =>
                {
                    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        string[] allowedOrigins = configuration.GetValue<string>("Cors:AllowedOrigins")
            ?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            ?? [];

        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return services;
    }
}
