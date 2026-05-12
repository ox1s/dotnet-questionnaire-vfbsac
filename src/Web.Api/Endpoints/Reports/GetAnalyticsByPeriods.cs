using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetAnalyticsByPeriods;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class GetAnalyticsByPeriods : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/analytics/periods", async (
            GetAnalyticsByPeriodsQuery query,
            IQueryHandler<GetAnalyticsByPeriodsQuery, List<GetAnalyticsByPeriodsQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<GetAnalyticsByPeriodsQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Reports")
        .RequireAuthorization();
    }
}
