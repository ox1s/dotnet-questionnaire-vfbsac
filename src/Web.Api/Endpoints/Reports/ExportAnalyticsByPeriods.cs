using Application.Abstractions.Messaging;
using Application.Reports.Commands.ExportAnalyticsByPeriods;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class ExportAnalyticsByPeriods : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/analytics/periods/export", async (
            ExportAnalyticsByPeriodsCommand command,
            ICommandHandler<ExportAnalyticsByPeriodsCommand, byte[]> handler,
            CancellationToken cancellationToken) =>
        {
            Result<byte[]> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                bytes => Results.File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"analytics-periods-{DateTime.UtcNow:yyyy-MM-dd}.docx"),
                CustomResults.Problem);
        })
        .WithTags("Reports")
        .RequireAuthorization();
    }
}
