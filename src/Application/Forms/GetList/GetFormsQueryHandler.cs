using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.GetList;

internal sealed class GetFormsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetFormsQuery, List<FormListItemResponse>>
{
    public async Task<Result<List<FormListItemResponse>>> Handle(GetFormsQuery query, CancellationToken cancellationToken)
    {
        List<FormListItemResponse> forms = await context.Forms
            .Where(f => query.IsActive == null || f.IsActive == query.IsActive)
            .OrderBy(f => f.Title)
            .Select(f => new FormListItemResponse
            {
                Id = f.Id,
                Title = f.Title,
                IsActive = f.IsActive,
                RequiredFilters = f.RequiredFilters
            })
            .ToListAsync(cancellationToken);

        return forms;
    }
}
