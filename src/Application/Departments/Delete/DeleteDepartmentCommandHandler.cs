using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.DepartmentAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.Delete;

internal sealed class DeleteDepartmentCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteDepartmentCommand>
{
    public async Task<Result> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        Department? department = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == command.DepartmentId, cancellationToken);

        if (department is null)
        {
            return Result.Failure(DepartmentErrors.NotFound(command.DepartmentId));
        }

        department.IsDeleted = true;
        context.Departments.Update(department);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
