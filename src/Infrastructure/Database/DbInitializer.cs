using Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database;

public class DbInitializer(
    IServiceProvider serviceProvider,
    ILogger<DbInitializer> logger,
    DemoDataGenerator demoDataGenerator,
    IHostEnvironment hostEnvironment)
{
    public async Task InitializeAsync()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPasswordHasher passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        if (hostEnvironment.IsProduction())
        {
            logger.LogInformation("Production environment detected; skipping demo data seeding.");
            return;
        }

        if (await context.Forms.AnyAsync())
        {
            logger.LogInformation("Database already initialized.");
            return;
        }

        logger.LogInformation("Seeding database...");
        string defaultPass = passwordHasher.Hash("12345678");
        await demoDataGenerator.SeedAsync(context, defaultPass);

        logger.LogInformation("Seeding completed with demo data.");
    }
}
