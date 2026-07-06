using Application.Abstractions.Messaging;
using Application.Teachers.Restore;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Teachers;

internal sealed class Restore : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("teachers/{teacherId:guid}/restore", async (
            Guid teacherId,
            ICommandHandler<RestoreTeacherCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreTeacherCommand(teacherId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
