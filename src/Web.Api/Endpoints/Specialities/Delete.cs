using Application.Abstractions.Messaging;
using Application.Specialities.Delete;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Specialities;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("specialities/{specialityId:guid}", async (
            Guid specialityId,
            ICommandHandler<DeleteSpecialityCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteSpecialityCommand(specialityId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
