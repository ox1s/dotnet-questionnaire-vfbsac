using Application.Abstractions.Authentication; // Добавить
using Application.Abstractions.Messaging;
using Application.Submissions.Create;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Submissions;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("submissions", async (
            CreateSubmissionCommand command,
            IUserContext userContext,
            ICommandHandler<CreateSubmissionCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            CreateSubmissionCommand secureCommand = command with { UserId = userContext.UserId };

            Result<Guid> result = await handler.Handle(secureCommand, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Submissions")
        .HasPermission(Permissions.SubmitForms);
    }
}
