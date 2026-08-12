using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class GetAnalyticsByPeriod : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/analytics/period", async (
            GetAnalyticsByPeriodQuery query,
            IQueryHandler<GetAnalyticsByPeriodQuery,
            GetAnalyticsByPeriodQueryResult> handler,
            CancellationToken cancellationToken) =>
        {
            Result<GetAnalyticsByPeriodQueryResult> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Reports")
        .HasPermission(Permissions.ReportsView);
    }
}
