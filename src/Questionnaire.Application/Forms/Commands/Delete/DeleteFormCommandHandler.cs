using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Forms;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Forms.Commands.Delete;

internal sealed class DeleteFormCommandHandler : ICommandHandler<DeleteFormCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteFormCommand command, CancellationToken cancellationToken)
    {
        var form = await _context.Forms.FindAsync([command.Id], cancellationToken);
        if (form is null)
        {
            return Result.Failure(FormErrors.NotFound(command.Id));
        }

        _context.Forms.Remove(form);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}