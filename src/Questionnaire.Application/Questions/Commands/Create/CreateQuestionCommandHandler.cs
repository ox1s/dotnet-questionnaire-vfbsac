using ErrorOr;
using MediatR;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Questions.Commands.Create;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, ErrorOr<Question>>
{
    private readonly IApplicationDbContext _context;

    public CreateQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Question>> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        // Бизнес-правило: варианты ответов могут быть только у вопроса типа Choice
        if (command.Type != QuestionType.Choice && command.Options is { Count: > 0 })
        {
            return Error.Validation(
                code: "Question.InvalidOptions",
                description: "Options can only be provided for Choice question type.");
        }

        var question = new Question
        {
            Text = command.Text,
            Type = command.Type
        };

        if (command.Type == QuestionType.Choice && command.Options is not null)
        {
            foreach (var optionText in command.Options)
            {
                question.Options.Add(new QuestionOption { Text = optionText });
            }
        }

        await _context.Questions.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return question;
    }
}