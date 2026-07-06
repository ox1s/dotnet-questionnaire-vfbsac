using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.Restore;

internal sealed class RestoreDepartmentCommandHandler(IApplicationDbContext context)
    : ICommandHandler<RestoreDepartmentCommand>
{
    public async Task<Result> Handle(RestoreDepartmentCommand command, CancellationToken cancellationToken)
    {
        Department? department = await context.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == command.DepartmentId, cancellationToken);

        if (department is null)
        {
            return Result.Failure(DepartmentErrors.NotFound(command.DepartmentId));
        }

        department.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
