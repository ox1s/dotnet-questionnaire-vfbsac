using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Teachers;
using SharedKernel;

namespace Application.Teachers.Create;

internal sealed class CreateTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTeacherCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTeacherCommand command, CancellationToken cancellationToken)
    {
        Result<Teacher> teacherResult = Teacher.Create(command.FullName);

        if (teacherResult.IsFailure)
        {
            return Result.Failure<Guid>(teacherResult.Error);
        }

        context.Teachers.Add(teacherResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return teacherResult.Value.Id;
    }
}
