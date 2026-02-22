using Application.Abstractions.Messaging;
using Application.Submissions.GetList;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Submissions;

internal sealed class GetList : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("submissions", async (
            [AsParameters] GetSubmissionsQuery query,
            IQueryHandler<GetSubmissionsQuery, List<SubmissionResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<SubmissionResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Submissions")
        .RequireAuthorization(Permissions.ReportsView);
    }
}
