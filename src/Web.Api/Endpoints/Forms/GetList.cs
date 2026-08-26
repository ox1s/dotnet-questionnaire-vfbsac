using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Forms.GetList;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Forms;

internal sealed class GetList : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("forms", async (
            IUserContext userContext,
            IQueryHandler<GetFormsQuery, List<GetFormsQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFormsQuery(IsActive: true, CallerRole: userContext.Role);

            Result<List<GetFormsQueryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Forms")
        .RequireAuthorization();
    }
}
