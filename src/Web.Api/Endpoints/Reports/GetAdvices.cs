using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetAdvices;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reports;

internal sealed class GetAdvices : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("reports/forms/{formId:guid}/advices", async (
            Guid formId,
            Guid? teacherId,
            IQueryHandler<GetAdvicesQuery, List<AdvicesQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAdvicesQuery(formId, teacherId);
            Result<List<AdvicesQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Reports")
        .RequireAuthorization();
    }
}
