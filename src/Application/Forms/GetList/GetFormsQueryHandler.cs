using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.GetList;

internal sealed class GetFormsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetFormsQuery, List<GetFormsQueryResponse>>
{
    public async Task<Result<List<GetFormsQueryResponse>>> Handle(GetFormsQuery query, CancellationToken cancellationToken)
    {
        // Teachers are college staff too (personnel category ППС), so they also get Staff forms.
        UserRole? alsoVisibleRole = query.CallerRole == UserRole.Teacher ? UserRole.Staff : query.CallerRole;

        List<GetFormsQueryResponse> forms = await context.Forms
            .Where(f => query.IsActive == null || f.IsActive == query.IsActive)
            .Where(f => query.CallerRole == null
                || f.TargetRole == null
                || f.TargetRole == query.CallerRole
                || f.TargetRole == alsoVisibleRole)
            .OrderBy(f => f.Title)
            .Select(f => new GetFormsQueryResponse
            {
                Id = f.Id,
                Title = f.Title,
                IsActive = f.IsActive,
                RequiredFilters = f.RequiredFilters,
                TargetRole = f.TargetRole
            })
            .ToListAsync(cancellationToken);

        return forms;
    }
}
