using Application.Abstractions.Messaging;
using Application.Users.GetAdmins;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetAdmins : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/admins", async (
            IQueryHandler<GetAdminsQuery, List<GetAdminsQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAdminsQuery();

            Result<List<GetAdminsQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
