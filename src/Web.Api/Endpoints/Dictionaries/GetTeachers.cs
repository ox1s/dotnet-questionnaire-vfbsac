using Application.Abstractions.Messaging;
using Application.Teachers.GetList; 
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Dictionaries;

internal sealed class GetTeachers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dictionaries/teachers", async (
            IQueryHandler<GetTeachersQuery, List<GetTeachersQueryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTeachersQuery();
            Result<List<GetTeachersQueryResponse>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .RequireAuthorization();
    }
}
