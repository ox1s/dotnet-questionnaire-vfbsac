using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.Deactivate;

internal sealed class DeactivateFormCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeactivateFormCommand>
{
    public async Task<Result> Handle(DeactivateFormCommand command, CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure(FormErrors.NotFound(command.FormId));
        }

        Result result = form.Deactivate();

        if (result.IsFailure)
        {
            return result;
        }

        context.Forms.Update(form);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
