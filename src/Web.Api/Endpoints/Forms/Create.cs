using Application.Abstractions.Messaging;
using Application.Forms.Create;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Forms;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("forms", async (
            CreateFormCommand command,
            ICommandHandler<CreateFormCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return CustomResults.Problem(result);
            }

            return Results.Ok(result.Value);
        })
        .WithTags("Forms")
        .RequireAuthorization(); // TODO: Добавить проверку на Админа
    }
}
