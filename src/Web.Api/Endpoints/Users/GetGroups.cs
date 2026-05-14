using Application.Abstractions.Messaging;
using Application.Users.GetGroups;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetGroups : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/groups", async (
            IQueryHandler<GetGroupsQuery, List<GetGroupsQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetGroupsQuery();

            Result<List<GetGroupsQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
