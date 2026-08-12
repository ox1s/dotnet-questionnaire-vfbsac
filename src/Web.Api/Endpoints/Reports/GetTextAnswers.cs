using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetTextAnswers;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class GetTextAnswers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reports/analytics/text-answers", async (
            GetTextAnswersQuery query,
            IQueryHandler<GetTextAnswersQuery, List<GetTextAnswersQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<GetTextAnswersQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Reports")
        .HasPermission(Permissions.ReportsView);
    }
}
