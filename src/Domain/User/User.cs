using System.Security.Cryptography;
using SharedKernel;
using Throw;

namespace Domain.User;

public sealed class User : Entity, ISoftDeletable
{
    public Login Login { get; private set; }
    public string PasswordHash { get; private set; }

    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public string DisplayName { get; private set; }
    public bool IsDeleted { get; set; }

    public Guid? DepartmentId { get; private set; }
    public Guid? GroupId { get; private set; }
    public Guid? TeacherId { get; private set; }
    public string? OrganizationName { get; private set; }


    private User() { } // EF Core

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
            DepartmentId = departmentId ?? null,
            GroupId = null,
            OrganizationName = null,
            IsActive = true
        };
    }

    public static Result<User> CreateEmployer(
        Login login,
        string displayName,
        string organizationName,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<User>(Error.NullValue);
        }

        if (string.IsNullOrWhiteSpace(organizationName))
        {
            return Result.Failure<User>(Error.NullValue);
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            DisplayName = displayName.Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.Employer,
            TeacherId = null,
            DepartmentId = null,
            GroupId = null,
            OrganizationName = organizationName.Trim(),
            IsActive = true
        };
    }

    public static Result<User> CreateAdmin(Login login, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Login = new Login(login.Value),
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

    public Result UpdateDetails(Login login, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure(Error.NullValue);
        }

        Login = login;
        DisplayName = displayName;

        return Result.Success();
    }

    public void UpdateOrganizationName(string organizationName)
    {
        OrganizationName = organizationName.Trim();
    }

    public void SetDepartment(Guid departmentId)
    {
        departmentId.ThrowIfNull();
        DepartmentId = departmentId;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
