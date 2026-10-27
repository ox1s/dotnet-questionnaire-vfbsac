using Application.Abstractions.Messaging;
using Application.Users.GetStaff;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetStaff : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/staff", async (
            IQueryHandler<GetStaffQuery, List<GetStaffQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStaffQuery();

            Result<List<GetStaffQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.Admin);
    }
}
