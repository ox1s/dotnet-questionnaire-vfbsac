using Application.Abstractions.Messaging;
using Application.Specializations.Restore;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Specializations;

internal sealed class Restore : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("specializations/{specializationId:guid}/restore", async (
            Guid specializationId,
            ICommandHandler<RestoreSpecializationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreSpecializationCommand(specializationId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
