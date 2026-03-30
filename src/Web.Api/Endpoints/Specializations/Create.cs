using Application.Abstractions.Messaging;
using Application.Specializations.Create;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Specializations;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("specializations", async (
            CreateSpecializationCommand command,
            ICommandHandler<CreateSpecializationCommand, Guid> handler,
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
