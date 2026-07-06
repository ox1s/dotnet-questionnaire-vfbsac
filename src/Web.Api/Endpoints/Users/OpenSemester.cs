using Application.Abstractions.Messaging;
using Application.Users.OpenSemester;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public sealed class OpenSemester : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("settings/open-semester", async (
            ICommandHandler<OpenSemesterCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new OpenSemesterCommand();
            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(() => Results.Ok(), CustomResults.Problem);
        })
        .WithTags(Tags.Settings)
        .HasPermission(Permissions.Admin);
    }
}
