using Application.Abstractions.Messaging;
using Application.Disciplines.GetList; // Убедитесь, что namespace верный
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Dictionaries;

internal sealed class GetDisciplines : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dictionaries/disciplines", async (
            IQueryHandler<GetDisciplinesQuery, List<DisciplineResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDisciplinesQuery();
            Result<List<DisciplineResponse>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .RequireAuthorization();
    }
}
