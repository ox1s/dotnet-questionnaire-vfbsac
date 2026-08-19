using Application.Abstractions.Messaging;
using Application.Forms.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Forms;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("forms/{formId:guid}", async (
            Guid formId,
            ICommandHandler<DeleteFormCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteFormCommand(formId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Forms")
        .HasPermission(Permissions.Admin);
    }
}
