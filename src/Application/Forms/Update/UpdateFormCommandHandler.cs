using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.Update;

internal sealed class UpdateFormCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateFormCommand>
{
    public async Task<Result> Handle(UpdateFormCommand command, CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure(FormErrors.NotFound(command.FormId));
        }

        form.UpdateTitle(command.Title);

        if (command.IsActive.HasValue)
        {
            if (command.IsActive.Value)
            {
                form.Activate();
            }
            else
            {
                form.Deactivate();
            }
        }

        if (command.RequiredFilters is not null)
        {
            form.UpdateRequiredFilters(command.RequiredFilters);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
