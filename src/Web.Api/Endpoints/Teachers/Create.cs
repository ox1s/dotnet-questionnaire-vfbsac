using Application.Abstractions.Messaging;
using Application.Teachers.Create;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Teachers;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("teachers", async (
            CreateTeacherCommand command,
            ICommandHandler<CreateTeacherCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return CustomResults.Problem(result);
            }

            return Results.Ok(result.Value);
        })
        .WithTags("Teachers") // Группировка в Swagger
        .RequireAuthorization(); // Желательно добавить политику админа, например .RequireAuthorization("AdminPolicy")
    }
}
