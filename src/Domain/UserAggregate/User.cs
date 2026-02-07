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


    public Guid? DepartmentId { get; private set; }
    public Guid? GroupId { get; private set; }   // Если Role == StudentGroup
    public Guid? TeacherId { get; private set; } // Если Role == Teacher
    public string? OrganizationName { get; private set; } // Если Role == Employer


    private User() { }

    public static Result<User> CreateGroupUser(
        GroupName groupName,
        Guid groupId,
        string passwordHash)
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
        Guid? teacherId,
        Guid? departmentId,
        string passwordHash,
        UserRole role = UserRole.Staff)
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
            Role = role,
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

    public void SetPasswordByAdmin(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void UpdateDetails(Login login, string displayName)
    {
        Login = login;
        DisplayName = displayName;
    }

    public void SetDepartment(Guid departmentId)
    {
        DepartmentId = departmentId;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
