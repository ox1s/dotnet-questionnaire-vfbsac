using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Forms;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Forms.Commands.RemoveQuestion;

internal sealed class RemoveQuestionFromFormCommandHandler : ICommandHandler<RemoveQuestionFromFormCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveQuestionFromFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveQuestionFromFormCommand command, CancellationToken cancellationToken)
    {
        var formQuestion = await _context.FormQuestions
            .FirstOrDefaultAsync(fq => fq.FormId == command.FormId && fq.QuestionId == command.QuestionId, cancellationToken);

        if (formQuestion is null)
        {
            return Result.Failure(FormErrors.QuestionNotFound(command.QuestionId));
        }

        _context.FormQuestions.Remove(formQuestion);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}