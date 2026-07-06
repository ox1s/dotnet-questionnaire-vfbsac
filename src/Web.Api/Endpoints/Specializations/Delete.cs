using Application.Abstractions.Messaging;
using Application.Specializations.Delete;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Specializations;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("specializations/{specializationId:guid}", async (
            Guid specializationId,
            ICommandHandler<DeleteSpecializationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteSpecializationCommand(specializationId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
