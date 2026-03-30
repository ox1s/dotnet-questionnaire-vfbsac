using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
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
        Result<GroupName> groupNameResult = GroupName.Create(command.GroupName);
        if (groupNameResult.IsFailure)
        {
            return Result.Failure<Guid>(groupNameResult.Error);
        }
        GroupName groupName = groupNameResult.Value;    
        string login = groupName.Value;

        bool exists = await context.Users
            .AnyAsync(u => u.Login.Value == login, cancellationToken);
        if (exists)
        {
            return Result.Failure<Guid>(UserErrors.GroupExists(login));
        }
        
        string passwordHash = passwordHasher.Hash(command.Password);

        Result<User> userResult = User.CreateGroupUser(groupNameResult.Value, Guid.NewGuid(), passwordHash);

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        context.Users.Add(userResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return userResult.Value.Id;
    }
}
