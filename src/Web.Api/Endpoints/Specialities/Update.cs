using Application.Abstractions.Messaging;
using Application.Specialities.Update;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Specialities;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("specialities/{specialityId:guid}", async (
            Guid specialityId,
            UpdateSpecialityRequest request,
            ICommandHandler<UpdateSpecialityCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSpecialityCommand(specialityId, request.Name);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }

    public sealed record UpdateSpecialityRequest(string Name);
}
