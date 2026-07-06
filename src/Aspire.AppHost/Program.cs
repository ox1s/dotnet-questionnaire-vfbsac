using Aspire.Hosting.DevTunnels;
using Aspire.Hosting.JavaScript;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithHostPort(5435)
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> database = postgres
    .AddDatabase("questionnaire-vfbsac");

IResourceBuilder<ProjectResource> backend = builder.AddProject<Projects.Web_Api>("web-api")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(database)
    .WaitFor(database);


IResourceBuilder<ViteAppResource> frontend = builder.AddViteApp("frontend", "../Web.Client")
    .WithExternalHttpEndpoints();

builder.AddDevTunnel("public-api")
    .WithReference(backend)
    .WithReference(frontend)
    .WithAnonymousAccess();

builder.Build().Run();
