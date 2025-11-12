using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Commands.AddQuestion;

public class AddQuestionToFormCommandHandler : IRequestHandler<AddQuestionToFormCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public AddQuestionToFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(AddQuestionToFormCommand command, CancellationToken cancellationToken)
    {
        var form = await _context.Forms.FindAsync(command.FormId);
        if (form is null)
        {
            return Error.NotFound(description: "Form not found.");
        }

        var question = await _context.Questions.FindAsync(command.QuestionId);
        if (question is null)
        {
            return Error.NotFound(description: "Question not found.");
        }

        var alreadyExists = await _context.FormQuestions
            .AnyAsync(fq => fq.FormId == command.FormId && fq.QuestionId == command.QuestionId, cancellationToken);

        if (alreadyExists)
        {
            return Error.Conflict(description: "This question is already in the form.");
        }

        var formQuestion = new FormQuestion
        {
            FormId = command.FormId,
            QuestionId = command.QuestionId,
            Order = command.Order
        };

        await _context.FormQuestions.AddAsync(formQuestion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}