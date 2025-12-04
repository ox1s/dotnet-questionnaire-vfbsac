using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Forms;
using Questionnaire.Domain.Questions;
using Questionnaire.Domain.Entities;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Forms.Commands.AddQuestion;

internal sealed class AddQuestionToFormCommandHandler : ICommandHandler<AddQuestionToFormCommand>
{
    private readonly IApplicationDbContext _context;

    public AddQuestionToFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AddQuestionToFormCommand command, CancellationToken cancellationToken)
    {
        var form = await _context.Forms.FindAsync([command.FormId], cancellationToken);
        if (form is null)
        {
            return Result.Failure(FormErrors.NotFound(command.FormId));
        }

        var question = await _context.Questions.FindAsync([command.QuestionId], cancellationToken);
        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound(command.QuestionId));
        }

        var alreadyExists = await _context.FormQuestions
            .AnyAsync(fq => fq.FormId == command.FormId && fq.QuestionId == command.QuestionId, cancellationToken);

        if (alreadyExists)
        {
            return Result.Failure(FormErrors.QuestionAlreadyExists(command.QuestionId));
        }

        var formQuestion = new FormQuestion
        {
            FormId = command.FormId,
            QuestionId = command.QuestionId,
            Order = command.Order
        };

        await _context.FormQuestions.AddAsync(formQuestion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}