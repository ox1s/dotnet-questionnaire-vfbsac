using Application.Abstractions.Messaging;
using Application.Users.Register;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Register : IEndpoint
{
    private sealed record Request(string Login, string DisplayName, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
                Request request,
                ICommandHandler<RegisterUserCommand, Guid> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RegisterUserCommand(
                    request.Login,
                    request.DisplayName,
                    request.Password);

                Result<Guid> result = await handler.Handle(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .AllowAnonymous()
            .WithTags(Tags.Users);
    }
}
