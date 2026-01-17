using Application.Abstractions.Messaging;
using Application.Disciplines.Delete;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Disciplines;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("disciplines/{disciplineId:guid}", async (
            Guid disciplineId,
            ICommandHandler<DeleteDisciplineCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteDisciplineCommand(disciplineId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
