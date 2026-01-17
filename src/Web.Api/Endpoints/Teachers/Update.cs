using Application.Abstractions.Messaging;
using Application.Teachers.Update;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Teachers;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("teachers/{teacherId:guid}", async (
            Guid teacherId,
            UpdateTeacherRequest request,
            ICommandHandler<UpdateTeacherCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateTeacherCommand(teacherId, request.FullName, request.DepartmentId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Teachers")
        .HasPermission(Permissions.Admin);

    }

    public sealed record UpdateTeacherRequest(string FullName, Guid DepartmentId);
}
