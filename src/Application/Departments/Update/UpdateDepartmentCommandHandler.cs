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

        // TODO: Implement this update with same name for others
        Department? departmentWithSameName = await context.Departments.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Name == command.Name && d.Id != command.DepartmentId, cancellationToken);

        if (departmentWithSameName is not null)
        {
            return Result.Failure(DepartmentErrors.Duplicate);
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
