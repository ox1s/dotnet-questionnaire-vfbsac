using Application.Abstractions.Messaging;
using Application.Forms.Activate;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Forms;

internal sealed class Activate : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("forms/{formId:guid}/activate", async (
            Guid formId,
            ICommandHandler<ActivateFormCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ActivateFormCommand(formId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Forms")
        .HasPermission(Permissions.Admin);
    }
}
