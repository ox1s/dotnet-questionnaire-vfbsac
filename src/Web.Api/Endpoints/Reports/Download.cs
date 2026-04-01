using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalytics;
using Infrastructure.Reports;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class Download : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/word", async (
                GetAnalyticsReportQuery query,
                IQueryHandler<GetAnalyticsReportQuery, AnalyticsReportResponse> handler,
                IReportGenerator reportGenerator,
                CancellationToken cancellationToken) =>
            {
                Result<AnalyticsReportResponse> result = await handler.Handle(query, cancellationToken);

                if (result.IsFailure)
                {
                    return CustomResults.Problem(result);
                }

                byte[] fileBytes = reportGenerator.GenerateAnalyticsReport(result.Value);

                return Results.File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"report_{query.FormId}.docx");
            })
            .WithTags("Reports")
            .RequireAuthorization();
    }
}
