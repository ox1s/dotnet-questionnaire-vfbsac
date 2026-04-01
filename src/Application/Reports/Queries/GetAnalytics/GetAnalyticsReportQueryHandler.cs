using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class GetAnalyticsReportQueryHandler(
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

        try
        {
            AnalyticsReportResponse response = await analyticsReportBuilder.BuildAsync(
                query.FormId,
                query.Slices,
                cancellationToken);

            return response;
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure<AnalyticsReportResponse>(
                Error.NotFound("Analytics.FormNotFound", exception.Message));
        }
    }
}
