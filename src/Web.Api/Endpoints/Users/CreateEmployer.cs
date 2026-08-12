using Application.Abstractions.Messaging;
using Application.Users.CreateEmployer;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class CreateEmployer : IEndpoint
{
    public sealed record CreateEmployerRequest(
        string Login,
        string DisplayName,
        string OrganizationName,
        string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/employers", async (
            CreateEmployerRequest request,
            ICommandHandler<CreateEmployerUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateEmployerUserCommand(
                Login: request.Login,
                DisplayName: request.DisplayName,
                OrganizationName: request.OrganizationName,
                Password: request.Password
            );
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
