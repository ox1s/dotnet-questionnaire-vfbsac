using Application.Abstractions.Messaging;
using Application.Disciplines.Create;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Disciplines;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("disciplines", async (
            CreateDisciplineCommand command,
            ICommandHandler<CreateDisciplineCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return CustomResults.Problem(result);
            }

            return Results.Ok(result.Value);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
