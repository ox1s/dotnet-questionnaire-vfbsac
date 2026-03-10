using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.DepartmentAggregate;
using Domain.College.TeacherAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.Create;

internal sealed class CreateTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTeacherCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTeacherCommand command, CancellationToken cancellationToken)
    {
        bool departmentExists = await context.Departments
            .AnyAsync(d => d.Id == command.DepartmentId, cancellationToken);

        if (!departmentExists)
        {
            return Result.Failure<Guid>(DepartmentErrors.NotFound(command.DepartmentId));
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
