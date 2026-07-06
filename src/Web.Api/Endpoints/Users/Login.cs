using Application.Abstractions.Messaging;
using Application.Users.SignIn;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Login : IEndpoint
{
    private sealed record Request(string Login, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login", async (
                Request request,
                ICommandHandler<LoginUserCommand, string> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new LoginUserCommand(request.Login, request.Password);

                Result<string> result = await handler.Handle(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .AllowAnonymous()
            .WithTags(Tags.Users);
    }
}
