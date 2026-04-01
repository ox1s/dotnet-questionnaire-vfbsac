using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetAnalytics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class GetAnalytics : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/analytics", async (
            GetAnalyticsReportQuery query,
            IQueryHandler<GetAnalyticsReportQuery, AnalyticsReportResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<AnalyticsReportResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Reports")
        .RequireAuthorization();
    }
}
