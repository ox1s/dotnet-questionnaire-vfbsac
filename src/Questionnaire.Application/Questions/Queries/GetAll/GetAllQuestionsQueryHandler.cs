using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Questions.Queries.GetAll;

public class GetAllQuestionsQueryHandler : IRequestHandler<GetAllQuestionsQuery, ErrorOr<IEnumerable<Question>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllQuestionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IEnumerable<Question>>> Handle(GetAllQuestionsQuery request, CancellationToken cancellationToken)
    {
        var questions = await _context.Questions
            .Include(q => q.Options)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return questions;
    }
}