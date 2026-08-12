using Application.Abstractions.Messaging;
using Application.Forms.Deactivate;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Forms;

internal sealed class Deactivate : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("forms/{formId:guid}/deactivate", async (
            Guid formId,
            ICommandHandler<DeactivateFormCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeactivateFormCommand(formId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Forms")
        .HasPermission(Permissions.Admin);
    }
}
