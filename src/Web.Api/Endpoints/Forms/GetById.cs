using Application.Abstractions.Messaging;
using Application.Forms.GetById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Forms;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("forms/{formId:guid}", async (
            Guid formId,
            IQueryHandler<GetFormByIdQuery, GetFormByIdQueryResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFormByIdQuery(formId);

            Result<GetFormByIdQueryResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Forms")
        .RequireAuthorization();
    }
}
