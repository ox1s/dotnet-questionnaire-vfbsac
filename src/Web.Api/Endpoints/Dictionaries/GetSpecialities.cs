using Application.Abstractions.Messaging;
using Application.Specialities.GetList;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Dictionaries;

internal sealed class GetSpecialities : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dictionaries/specialities", async (
            IQueryHandler<GetSpecialitiesQuery, List<SpecialityResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSpecialitiesQuery();
            Result<List<SpecialityResponse>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .RequireAuthorization();
    }
}
