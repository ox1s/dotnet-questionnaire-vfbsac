using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.CreateGroup;

internal sealed class CreateGroupUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher)
    : ICommandHandler<CreateGroupUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateGroupUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Валидация имени группы (Domain Value Object)
        Result<GroupName> groupNameResult = GroupName.Create(command.GroupName);
        if (groupNameResult.IsFailure)
        {
            return Result.Failure<Guid>(groupNameResult.Error);
        }

        // 2. Проверка на уникальность логина
        // Логин группы = Название группы
        string login = groupNameResult.Value.Value;

        bool exists = await context.Users.AnyAsync(u => u.Login.Value == login, cancellationToken);
        if (exists)
        {
            return Result.Failure<Guid>(UserErrors.Conflict("Users.GroupExists", $"Группа {login} уже существует"));
        }

        // 3. Хеширование пароля
        string passwordHash = passwordHasher.Hash(command.Password);

        // 4. Создание пользователя (используем фабричный метод из User.cs)
        // Примечание: User.CreateGroupUser требует int groupId, но в текущей реализации User.cs
        // кажется, groupId используется как числовой ID из старой базы. 
        // Давайте пока передадим 0 или заглушку, либо доработаем User.cs, если это поле не обязательно.
        // В рамках MVP будем считать GroupId = 0 (так как у нас нет отдельной таблицы Groups, группа - это просто Юзер).

        Result<User> userResult = User.CreateGroupUser(groupNameResult.Value, 0, passwordHash);

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        context.Users.Add(userResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return userResult.Value.Id;
    }
}
