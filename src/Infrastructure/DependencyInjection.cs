using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Infrastructure.Authentication;
using Infrastructure.Database;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using StackExchange.Redis;
using SharedKernel;
using System.Text;
using Infrastructure.Reports;
using Microsoft.EntityFrameworkCore.Migrations;
using Application.Abstractions.Reports;
using Infrastructure.Users;
using Infrastructure.Authorization;

namespace Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            return services
                .AddServices()
                .AddDatabase(configuration)
                .AddHealthChecksInternal(configuration)
                .AddAuthenticationInternal(configuration)
                .AddAuthorizationInternal();
        }

        private IServiceCollection AddServices()
        {
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            // services.AddScoped<IReportGenerator, WordReportGenerator>();
            services.AddScoped<IReportGenerator, ExcelReportGenerator>();
            services.AddScoped<DemoDataGenerator>();

            services.AddScoped<DbInitializer>();

            return services;
        }

        private IServiceCollection AddDatabase(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("Database");

            services.AddDbContext<ApplicationDbContext>(options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }

        private IServiceCollection AddHealthChecksInternal(IConfiguration configuration)
        {
            services
                .AddHealthChecks()
                .AddNpgSql(configuration.GetConnectionString("Database")!);

            return services;
        }

        private IServiceCollection AddAuthenticationInternal(IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddHttpContextAccessor();
            services.AddScoped<IUserContext, UserContext>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenProvider, TokenProvider>();

            return services;
        }

        private IServiceCollection AddAuthorizationInternal()
        {
            services.AddAuthorization();
            services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            return services;
        }
    }
}
