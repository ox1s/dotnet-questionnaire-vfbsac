using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class GetAnalyticsReportQueryHandler(
    IApplicationDbContext context,
    IAnalyticsReportBuilder analyticsReportBuilder)
    : IQueryHandler<GetAnalyticsReportQuery, AnalyticsReportResponse>
{
    public async Task<Result<AnalyticsReportResponse>> Handle(
        GetAnalyticsReportQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Slices.Count == 0)
        {
            return Result.Failure<AnalyticsReportResponse>(
                Error.Validation("Analytics.SlicesRequired", "At least one analytics slice is required."));
        }

        return await analyticsReportBuilder.BuildAsync(
                query.FormId,
                query.Slices,
                cancellationToken);
    }
}
