using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Queries.GetById;

public class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, ErrorOr<Form>>
{
    private readonly IApplicationDbContext _context;

    public GetFormByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Form>> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.FormQuestions)
                .ThenInclude(fq => fq.Question)
                .ThenInclude(q => q.Options)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (form is null)
        {
            return Error.NotFound(description: "Form not found.");
        }
        
        form.FormQuestions = form.FormQuestions.OrderBy(fq => fq.Order).ToList();

        return form;
    }
}