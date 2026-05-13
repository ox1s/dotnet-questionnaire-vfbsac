using Application.Abstractions.Messaging;
using Application.Reports.Commands.ExportAnalyticsByGroups;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class ExportAnalyticsByGroups : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/analytics/groups/export", async (
            ExportAnalyticsByGroupsCommand command,
            ICommandHandler<ExportAnalyticsByGroupsCommand, byte[]> handler,
            CancellationToken cancellationToken) =>
        {
            Result<byte[]> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                bytes => Results.File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"analytics-groups-{DateTime.UtcNow:yyyy-MM-dd}.docx"),
                CustomResults.Problem);
        })
        .WithTags("Reports")
        .RequireAuthorization();
    }
}
