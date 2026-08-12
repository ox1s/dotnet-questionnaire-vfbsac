using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.Activate;

internal sealed class ActivateFormCommandHandler(IApplicationDbContext context)
    : ICommandHandler<ActivateFormCommand>
{
    public async Task<Result> Handle(ActivateFormCommand command, CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure(FormErrors.NotFound(command.FormId));
        }

        if (form.IsActive)
        {
            return Result.Failure(FormErrors.AlreadyActive(form.Id));
        }

        form.Activate();
        context.Forms.Update(form);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
