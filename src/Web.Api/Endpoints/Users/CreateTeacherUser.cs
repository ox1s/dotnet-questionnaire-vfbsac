using Application.Abstractions.Messaging;
using Application.Users.CreateTeacher;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class CreateTeacherUser : IEndpoint
{
    public sealed record CreateTeacherUserRequest(
        Guid TeacherId,
        string Login,
        string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/teachers", async (
            CreateTeacherUserRequest request,
            ICommandHandler<CreateTeacherUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTeacherUserCommand(
                TeacherId: request.TeacherId,
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
