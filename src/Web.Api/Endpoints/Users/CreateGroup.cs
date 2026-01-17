using Application.Abstractions.Messaging;
using Application.Users.CreateGroup;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class CreateGroup : IEndpoint
{
    public sealed record CreateGroupRequest(string GroupName, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/groups", async (
            CreateGroupRequest request,
            ICommandHandler<CreateGroupUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateGroupUserCommand(request.GroupName, request.Password);
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Users")
        .RequireAuthorization(); // Только админ
    }
}
