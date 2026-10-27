using Application.Abstractions.Messaging;
using Application.Users.CreateAdmin;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class CreateAdmin : IEndpoint
{
    public sealed record CreateAdminRequest(
        string Login,
        string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/admins", async (
            CreateAdminRequest request,
            ICommandHandler<CreateAdminUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateAdminUserCommand(
                Login: request.Login,
                Password: request.Password
            );
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
