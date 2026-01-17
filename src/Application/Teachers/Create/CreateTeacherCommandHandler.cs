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
        // 1. Проверяем, существует ли кафедра
        bool departmentExists = await context.Departments
            .AnyAsync(d => d.Id == command.DepartmentId, cancellationToken);

        if (!departmentExists)
        {
            return Result.Failure<Guid>(DepartmentErrors.NotFound(command.DepartmentId));
        }

        // 2. Создаем сущность через фабричный метод Домена
        Result<Teacher> teacherResult = Teacher.Create(command.FullName, command.DepartmentId);

        if (teacherResult.IsFailure)
        {
            return Result.Failure<Guid>(teacherResult.Error);
        }

        // 3. Сохраняем в БД
        context.Teachers.Add(teacherResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return teacherResult.Value.Id;
    }
}
