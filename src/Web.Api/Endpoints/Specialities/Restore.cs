using Application.Abstractions.Messaging;
using Application.Specialities.Restore;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Specialities;

internal sealed class Restore : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("specialities/{specialityId:guid}/restore", async (
            Guid specialityId,
            ICommandHandler<RestoreSpecialityCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreSpecialityCommand(specialityId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
