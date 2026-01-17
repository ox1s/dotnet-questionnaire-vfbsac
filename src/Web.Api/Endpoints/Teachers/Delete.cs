using Application.Abstractions.Messaging;
using Application.Teachers.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Teachers;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("teachers/{teacherId:guid}", async (
            Guid teacherId,
            ICommandHandler<DeleteTeacherCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteTeacherCommand(teacherId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Teachers")
        .RequireAuthorization();
    }
}
