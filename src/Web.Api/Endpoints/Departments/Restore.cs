using Application.Abstractions.Messaging;
using Application.Departments.Restore;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Departments;

internal sealed class Restore : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("departments/{departmentId:guid}/restore", async (
            Guid departmentId,
            ICommandHandler<RestoreDepartmentCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreDepartmentCommand(departmentId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .HasPermission(Permissions.Admin);
    }
}
