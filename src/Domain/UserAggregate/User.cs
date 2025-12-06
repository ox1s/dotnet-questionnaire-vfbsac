using System.Security.Cryptography;
using Domain.UserAggregate.Events;
using SharedKernel;

namespace Domain.UserAggregate;

public sealed class User : AggregateRoot
{
    public Login Login { get; private set; } // номер группы
    public string PasswordHash { get; private set; }

    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public string DisplayName { get; private set; }


    public int? GroupId { get; private set; }   // Если Role == StudentGroup
    public int? TeacherId { get; private set; } // Если Role == Teacher
    public string? OrganizationName { get; private set; } // Если Role == Employer
    
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiration { get; private set; }
    
    private User() { }

    public static Result<User> CreateGroupUser(GroupName groupName, int groupId, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Login = new Login(groupName.Value.ToUpperInvariant()),
            DisplayName = groupName.Value,
            PasswordHash = passwordHash,
            Role = UserRole.StudentGroup,
            GroupId = groupId,
            TeacherId = null,
            OrganizationName = null,
            IsActive = true
        };
    }
    public static Result<User> CreateStaff(
        Login login,
        string fullName,
        int teacherId,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<User>(Error.NullValue);
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            DisplayName = fullName.Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.Staff,
            TeacherId = teacherId,
            GroupId = null,
            OrganizationName = null,
            IsActive = true
        };
    }

    public static Result<User> CreateAdmin(Login login, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Login = new Login(login.Value.ToUpperInvariant()),
            PasswordHash = passwordHash,
            DisplayName = login.Value,
            Role = UserRole.Admin,
            IsActive = true
        };
    }



    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void RequestPasswordReset(IDateTimeProvider dateTimeProvider)
    {
        byte[] tokenBytes = new byte[32];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        PasswordResetToken = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        PasswordResetTokenExpiration = dateTimeProvider.UtcNow.AddHours(1);

        RaiseDomainEvent(new PasswordResetRequestedDomainEvent(Id, Login.Value, PasswordResetToken));
    }

    public Result ResetPassword(string token, string newPasswordHash, IDateTimeProvider dateTimeProvider)
    {
        if (PasswordResetToken != token)
        {
            return Result.Failure(UserErrors.InvalidResetToken);
        }

        if (dateTimeProvider.UtcNow > PasswordResetTokenExpiration)
        {
            return Result.Failure(UserErrors.ExpiredResetToken);
        }

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiration = null;

        return Result.Success();
    }
}
