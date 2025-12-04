using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Contracts.Questions;
using Questionnaire.Domain.Entities;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Questions.Commands.Create;

internal sealed class CreateQuestionCommandHandler : ICommandHandler<CreateQuestionCommand, QuestionResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<QuestionResponse>> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        var domainType = MapToDomainQuestionType(command.Type);
        
        // Бизнес-правило: варианты ответов могут быть только у вопроса типа Choice
        if (domainType != Domain.Entities.QuestionType.Choice && command.Options is { Count: > 0 })
        {
            return Result.Failure<QuestionResponse>(Error.Validation(
                "Question.InvalidOptions",
                "Options can only be provided for Choice question type."));
        }

        var question = new Question
        {
            Text = command.Text,
            Type = domainType
        };

        if (domainType == Domain.Entities.QuestionType.Choice && command.Options is not null)
        {
            foreach (string optionText in command.Options)
            {
                question.Options.Add(new QuestionOption { Text = optionText });
            }
        }

        await _context.Questions.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var options = question.Options.Select(o => new OptionResponse(o.Id, o.Text)).ToList();
        var response = new QuestionResponse(question.Id, question.Text, command.Type, options);

        return Result.Success(response);
    }

    private static Domain.Entities.QuestionType MapToDomainQuestionType(Contracts.Questions.QuestionType contractType)
    {
        return contractType switch
        {
            Contracts.Questions.QuestionType.Rating => Domain.Entities.QuestionType.Rating,
            Contracts.Questions.QuestionType.Text => Domain.Entities.QuestionType.Text,
            Contracts.Questions.QuestionType.Choice => Domain.Entities.QuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map contract question type to domain."),
        };
    }
}