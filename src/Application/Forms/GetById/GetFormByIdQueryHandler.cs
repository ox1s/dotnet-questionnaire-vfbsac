using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.GetById;

internal sealed class GetFormByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetFormByIdQuery, FormResponse>
{
    public async Task<Result<FormResponse>> Handle(GetFormByIdQuery query, CancellationToken cancellationToken)
    {
        FormResponse? form = await context.Forms
            .Where(f => f.Id == query.FormId)
            .Select(f => new FormResponse
            {
                Id = f.Id,
                Title = f.Title,
                IsActive = f.IsActive,
                RequiredFilters = f.RequiredFilters,
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
            return Result.Failure<FormResponse>(FormErrors.NotFound(query.FormId));
        }

        return form;
    }
}
