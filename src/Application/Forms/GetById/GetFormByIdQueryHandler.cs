using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.GetById;

internal sealed class GetFormByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetFormByIdQuery, GetFormByIdQueryResponse>
{
    public async Task<Result<GetFormByIdQueryResponse>> Handle(GetFormByIdQuery query, CancellationToken cancellationToken)
    {
        GetFormByIdQueryResponse? form = await context.Forms
            .Where(f => f.Id == query.FormId)
            .Select(f => new GetFormByIdQueryResponse
            {
                Id = f.Id,
                Title = f.Title,
                IsActive = f.IsActive,
                RequiredFilters = f.RequiredFilters,
                TargetRole = f.TargetRole,
                Questions = f.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new QuestionResponse
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Type = q.Type,
                        Order = q.Order
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (form is null)
        {
            return Result.Failure<GetFormByIdQueryResponse>(FormErrors.NotFound(query.FormId));
        }

        return form;
    }
}
