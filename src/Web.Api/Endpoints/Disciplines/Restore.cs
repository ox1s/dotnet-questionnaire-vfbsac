using Application.Abstractions.Messaging;
using Application.Disciplines.Restore;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Disciplines;

internal sealed class Restore : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("disciplines/{disciplineId:guid}/restore", async (
            Guid disciplineId,
            ICommandHandler<RestoreDisciplineCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreDisciplineCommand(disciplineId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
