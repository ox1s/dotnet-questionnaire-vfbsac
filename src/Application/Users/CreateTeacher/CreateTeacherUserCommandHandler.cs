using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Teachers;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.CreateTeacher;

internal sealed class CreateTeacherUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher)
    : ICommandHandler<CreateTeacherUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTeacherUserCommand command, CancellationToken cancellationToken)
    {
        Teacher? teacher = await context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == command.TeacherId, cancellationToken);

        if (teacher is null)
        {
            return Result.Failure<Guid>(TeacherErrors.NotFound(command.TeacherId));
        }

        if (await context.Users.AnyAsync(
                u => u.Role == UserRole.Teacher && u.TeacherId == command.TeacherId,
                cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.TeacherAccountExists(command.TeacherId));
        }

        Result<Login> loginResult = Login.Create(command.Login);
        if (loginResult.IsFailure)
        {
            return Result.Failure<Guid>(loginResult.Error);
        }

        if (await context.Users.AnyAsync(u => u.Login.Value == loginResult.Value.Value, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.UserExist());
        }

        string hash = passwordHasher.Hash(command.Password);

        Result<User> userResult = User.CreateTeacher(
            loginResult.Value,
            teacher.FullName,
            teacher.Id,
            hash
        );

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        context.Users.Add(userResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return userResult.Value.Id;
    }
}
