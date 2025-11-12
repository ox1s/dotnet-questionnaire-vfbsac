using ErrorOr;
using MediatR;
using Questionnaire.Application.Common.Interfaces;

namespace Questionnaire.Application.Questions.Commands.Delete;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public DeleteQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions.FindAsync(request.Id);
        if (question is null)
        {
            return Error.NotFound("Question not found.");
        }

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}