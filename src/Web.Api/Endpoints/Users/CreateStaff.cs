using Application.Abstractions.Messaging;
using Application.Users.CreateGroup;
using Application.Users.CreateStaff;
using Domain.User;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class CreateStaff : IEndpoint
{
    private sealed record CreateGroupRequest(
        string Login,
        string FullName,
        string Password,
        Guid? DepartmentId,
        UserRole Role);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/staff", async (
            CreateGroupRequest request,
            ICommandHandler<CreateStaffUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateStaffUserCommand(
                Login: request.Login,
                FullName: request.FullName,
                Password: request.Password,
                DepartmentId: request.DepartmentId,
                Role: request.Role
            );
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
