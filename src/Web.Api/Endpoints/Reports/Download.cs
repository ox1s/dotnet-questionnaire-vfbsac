using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Submissions.GetStatistics;
using Infrastructure.Reports;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class Download : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("reports/word/{formId:guid}", async (
                Guid formId,
                IQueryHandler<GetSubmissionStatisticsQuery, SubmissionStatisticsResponse> handler,
                IReportGenerator reportGenerator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetSubmissionStatisticsQuery(formId);
                Result<SubmissionStatisticsResponse> result = await handler.Handle(query, cancellationToken);

                if (result.IsFailure)
                {
                    return CustomResults.Problem(result);
                }

                // Исправлено: byte[] вместо var
                byte[] fileBytes = reportGenerator.GenerateFormReport("Результаты анкетирования", result.Value);

                return Results.File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"report_{formId}.docx");
            })
            .WithTags("Reports")
            .RequireAuthorization();
    }
}
