using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Domain.College.Teachers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.Create;

internal sealed class CreateTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTeacherCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTeacherCommand command, CancellationToken cancellationToken)
    {
        if (command.DepartmentId.HasValue)
        {
            bool departmentExists = await context.Departments
                .AnyAsync(d => d.Id == command.DepartmentId.Value, cancellationToken);

            if (!departmentExists)
            {
                return Result.Failure<Guid>(DepartmentErrors.NotFound(command.DepartmentId.Value));
            }
        }

        Result<Teacher> teacherResult = Teacher.Create(command.FullName, command.DepartmentId);

        if (teacherResult.IsFailure)
        {
            return Result.Failure<Guid>(teacherResult.Error);
        }

        context.Teachers.Add(teacherResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return teacherResult.Value.Id;
    }
}
