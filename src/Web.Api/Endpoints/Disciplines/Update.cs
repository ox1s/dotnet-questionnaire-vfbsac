using Application.Abstractions.Messaging;
using Application.Disciplines.Update;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Disciplines;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("disciplines/{disciplineId:guid}", async (
            Guid disciplineId,
            UpdateDisciplineRequest request,
            ICommandHandler<UpdateDisciplineCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateDisciplineCommand(disciplineId, request.Name, request.DepartmentId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
    public sealed record UpdateDisciplineRequest(string Name, Guid DepartmentId);
}
