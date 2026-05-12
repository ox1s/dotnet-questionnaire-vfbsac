using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetAnalyticsByGroups;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class GetAnalyticsByGroups : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/analytics/groups", async (
            GetAnalyticsByGroupsQuery query,
            IQueryHandler<GetAnalyticsByGroupsQuery, List<GetAnalyticsByGroupsQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<GetAnalyticsByGroupsQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Reports")
        .RequireAuthorization();
    }
}
