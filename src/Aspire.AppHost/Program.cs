IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithImage("postgres:17")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("questionnaire-vfbsac");

builder.AddProject<Projects.Web_Api>("web-api")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
