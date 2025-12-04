using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Contracts.Forms;
using Questionnaire.Contracts.Questions;
using Questionnaire.Domain.Forms;
using Questionnaire.Domain.Entities;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Forms.Queries.GetById;

internal sealed class GetFormByIdQueryHandler : IQueryHandler<GetFormByIdQuery, FormResponse>
{
    private readonly IApplicationDbContext _context;

    public GetFormByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FormResponse>> Handle(GetFormByIdQuery query, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.FormQuestions)
                .ThenInclude(fq => fq.Question)
                .ThenInclude(q => q.Options)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.Id, cancellationToken);

        if (form is null)
        {
            return Result.Failure<FormResponse>(FormErrors.NotFound(query.Id));
        }
        
        var orderedFormQuestions = form.FormQuestions.OrderBy(fq => fq.Order).ToList();
        var questions = orderedFormQuestions.Select(fq => MapToQuestionResponse(fq.Question)).ToList();

        var response = new FormResponse(
            form.Id,
            form.Name,
            form.IsActive,
            questions);

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