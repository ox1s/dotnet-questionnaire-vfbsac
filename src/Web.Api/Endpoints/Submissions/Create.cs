using Application.Abstractions.Messaging;
using Application.Submissions.Create;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Submissions;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("submissions", async (
            CreateSubmissionCommand command,
            ICommandHandler<CreateSubmissionCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            // Важно: UserId должен браться из токена, но пока берем из команды для простоты тестирования
            // В будущем здесь нужно переопределять command.UserId = userContext.UserId

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Submissions")
        .RequireAuthorization();
    }
}
