using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Questionnaire.Infrastructure.Persistence;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        
        try
        {
            ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
            ILogger<ApplicationDbContext> logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            logger.LogInformation("Applying database migrations...");
            
            await context.Database.MigrateAsync();
            
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            ILogger<DatabaseExtensions> errorLogger = services.GetRequiredService<ILogger<DatabaseExtensions>>();
            errorLogger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }
}
