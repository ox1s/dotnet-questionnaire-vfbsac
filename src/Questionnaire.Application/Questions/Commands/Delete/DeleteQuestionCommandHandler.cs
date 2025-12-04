using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Questions;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Questions.Commands.Delete;

internal sealed class DeleteQuestionCommandHandler : ICommandHandler<DeleteQuestionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteQuestionCommand command, CancellationToken cancellationToken)
    {
        var question = await _context.Questions.FindAsync([command.Id], cancellationToken);
        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound(command.Id));
        }

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}