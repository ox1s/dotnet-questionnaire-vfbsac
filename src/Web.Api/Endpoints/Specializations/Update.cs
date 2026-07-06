using Application.Abstractions.Messaging;
using Application.Specializations.Update;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Specializations;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("specializations/{specializationId:guid}", async (
            Guid specializationId,
            UpdateSpecializationRequest request,
            ICommandHandler<UpdateSpecializationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSpecializationCommand(
                specializationId,
                request.Name,
                request.SpecialityId);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }

    public sealed record UpdateSpecializationRequest(string Name, Guid SpecialityId);
}
