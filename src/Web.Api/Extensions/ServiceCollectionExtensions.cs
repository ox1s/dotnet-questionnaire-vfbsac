using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi; // Основной namespace для v2

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            // 1. Definition остается почти таким же
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter your JWT token in this field",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            };

            o.AddSecurityDefinition("Bearer", securityScheme); // Используем имя "Bearer"

            // 2. Requirement теперь требует функцию (doc => ...) и OpenApiSecuritySchemeReference
            o.AddSecurityRequirement(document =>
            {
                var requirement = new OpenApiSecurityRequirement
                {
                    // В v2 используем OpenApiSecuritySchemeReference для связи с Definition
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                };

                return requirement;
            });
        });

        return services;
    }
}
