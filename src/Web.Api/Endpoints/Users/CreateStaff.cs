using Application.Abstractions.Messaging;
using Application.Users.CreateStaff;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class CreateStaff : IEndpoint
{
    public sealed record CreateStaffRequest(
        string Login,
        string FullName,
        string Password,
        Guid? DepartmentId = null);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/staff", async (
            CreateStaffRequest request,
            ICommandHandler<CreateStaffUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateStaffUserCommand(
                Login: request.Login,
                FullName: request.FullName,
                Password: request.Password,
                DepartmentId: request.DepartmentId
            );
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
