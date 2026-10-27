using Application.Abstractions.Messaging;
using Application.Users.GetTeacherUsers;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetTeacherUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/teachers", async (
            IQueryHandler<GetTeacherUsersQuery, List<GetTeacherUsersQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTeacherUsersQuery();

            Result<List<GetTeacherUsersQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
