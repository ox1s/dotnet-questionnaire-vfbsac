using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.Create;

internal sealed class CreateDepartmentCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateDepartmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        Department existingDepartment = context.Departments.IgnoreQueryFilters().FirstOrDefault(d => d.Name == command.Name);
        if (existingDepartment is not null)
        {
            return Result.Failure<Guid>(DepartmentErrors.Duplicate);
        }

        Result<Department> departmentResult = Department.Create(command.Name);
        if (departmentResult.IsFailure)
        {
            return Result.Failure<Guid>(departmentResult.Error);
        }

        Department department = departmentResult.Value;
        context.Departments.Add(department);
        await context.SaveChangesAsync(cancellationToken);

        return department.Id;
    }
}
