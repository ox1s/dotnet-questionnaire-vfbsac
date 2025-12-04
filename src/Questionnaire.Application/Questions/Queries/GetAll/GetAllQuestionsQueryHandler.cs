using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Contracts.Questions;
using Questionnaire.Domain.Entities;
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
        var questionType = MapQuestionType(question.Type);
        
        return new QuestionResponse(
            question.Id,
            question.Text,
            questionType,
            options);
    }

    private static Contracts.Questions.QuestionType MapQuestionType(Domain.Entities.QuestionType domainType)
    {
        return domainType switch
        {
            Domain.Entities.QuestionType.Rating => Contracts.Questions.QuestionType.Rating,
            Domain.Entities.QuestionType.Text => Contracts.Questions.QuestionType.Text,
            Domain.Entities.QuestionType.Choice => Contracts.Questions.QuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map domain question type to contract."),
        };
    }
}