using Application.Abstractions.Messaging;
using Application.Departments.GetList;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Dictionaries;

internal sealed class GetDepartments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dictionaries/departments", async (
            IQueryHandler<GetDepartmentsQuery, List<GetDepartmentResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDepartmentsQuery();
            Result<List<GetDepartmentResponse>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .RequireAuthorization();
    }
}
