using Questionnaire.Api;
using Questionnaire.Application;
using Questionnaire.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
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
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}