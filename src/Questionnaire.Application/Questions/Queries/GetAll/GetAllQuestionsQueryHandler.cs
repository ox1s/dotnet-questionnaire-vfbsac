using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Application.Questions.Common;
using Questionnaire.Domain.Questions;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Questions.Queries.GetAll;

internal sealed class GetAllQuestionsQueryHandler : IQueryHandler<GetAllQuestionsQuery, IEnumerable<QuestionResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAllQuestionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> Handle(GetAllQuestionsQuery query, CancellationToken cancellationToken)
    {
        var questions = await _context.Questions
            .Include(q => q.Options)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = questions.Select(q => MapToQuestionResponse(q));

        return Result.Success(response);
    }

    private static QuestionResponse MapToQuestionResponse(Question question)
    {
        var options = question.Options.Select(o => new OptionResponse(o.Id, o.Text)).ToList();
        
        return new QuestionResponse(
            question.Id,
            question.Text,
            question.Type,
            options);
    }
}