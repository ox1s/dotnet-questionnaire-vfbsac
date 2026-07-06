using System;
using System.Threading;
using Application.Abstractions.Messaging;
using Application.Users.AdminSetPassword;
using Domain.User;
using Microsoft.AspNetCore.Routing;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;
using Web.Api.Middleware;

namespace Web.Api.Endpoints.Users;

internal sealed class AdminSetPassword : IEndpoint
{
    public sealed record Request(Guid UserId, string NewPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId:guid}/set-password", async (
                Guid userId,
                Request request,
                ICommandHandler<AdminSetPasswordCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new AdminSetPasswordCommand(userId, request.NewPassword);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .WithTags(Tags.Users)
            .HasPermission(Permissions.Admin);
    }

}
