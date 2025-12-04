using Serilog;
using Questionnaire.Api;
using Questionnaire.Api.Middleware;
using Questionnaire.Application;
using Questionnaire.Infrastructure;
using Questionnaire.Infrastructure.Persistence;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Настроить Serilog
// Конфигурация загружается из appsettings.json, дополнительные настройки только через DI
builder.Host.UseSerilog((context, services, configuration) => 
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));
{
    builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
        });

    builder.Services
        .AddPresentation()
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    // app.UseInfrastructure(); 

    if (app.Environment.IsDevelopment())
    {
        await app.ApplyMigrationsAsync();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRequestContextLogging();
    app.UseExceptionHandler();

    app.UseHttpsRedirection();
    app.UseCors(MyAllowSpecificOrigins);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");

    app.MapControllers();

    try
    {
        app.Run();
    }
    finally
    {
        // Обеспечить правильное закрытие Serilog при завершении приложения
        // даже при возникновении необработанных исключений
        Log.CloseAndFlush();
    }
}