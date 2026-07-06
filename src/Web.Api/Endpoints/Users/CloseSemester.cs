using Application.Abstractions.Messaging;
using Application.Users.CloseSemester;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public sealed class CloseSemester : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("settings/close-semester", async (
            ICommandHandler<CloseSemesterCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CloseSemesterCommand();
            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(() => Results.Ok(), CustomResults.Problem);
        })
        .WithTags(Tags.Settings)
        .HasPermission(Permissions.Admin);
    }
}
