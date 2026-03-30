using Application.Abstractions.Messaging;
using Application.Specializations.GetList;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Dictionaries;

internal sealed class GetSpecializations : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dictionaries/specializations", async (
            IQueryHandler<GetSpecializationsQuery, List<SpecializationResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSpecializationsQuery();
            Result<List<SpecializationResponse>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .RequireAuthorization();
    }
}
