using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.Update;

internal sealed class UpdateDepartmentCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateDepartmentCommand>
{
    public async Task<Result> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        Department? department = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == command.DepartmentId, cancellationToken);

        if (department is null)
        {
            return Result.Failure(DepartmentErrors.NotFound(command.DepartmentId));
        }

        Result updateNameResult = department.UpdateName(command.Name);
        if (updateNameResult.IsFailure)
        {
            return Result.Failure(updateNameResult.Error);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
