using Application.Abstractions.Messaging;
using Application.Departments.Delete;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Departments;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("departments/{departmentId:guid}", async (
            Guid departmentId,
            ICommandHandler<DeleteDepartmentCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteDepartmentCommand(departmentId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
