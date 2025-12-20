using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.Delete;

internal sealed class DeleteFormCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteFormCommand>
{
    public async Task<Result> Handle(DeleteFormCommand command, CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure(FormErrors.NotFound(command.FormId));
        }

        context.Forms.Remove(form);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
