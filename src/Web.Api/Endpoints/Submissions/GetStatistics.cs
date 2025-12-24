using Application.Abstractions.Messaging;
using Application.Submissions.GetStatistics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Submissions;

internal sealed class GetStatistics : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("submissions/statistics", async (
            [AsParameters] GetSubmissionStatisticsQuery query,
            IQueryHandler<GetSubmissionStatisticsQuery, SubmissionStatisticsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<SubmissionStatisticsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Submissions")
        .RequireAuthorization();
    }
}
