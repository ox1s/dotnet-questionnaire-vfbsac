using SharedKernel;
using Throw;

namespace Domain.College.Teacher;

public sealed class Teacher : Entity, ISoftDeletable
{
    public string FullName { get; private set; }
    public bool IsDeleted { get; set; }
    
    public Guid DepartmentId { get; private set; }

    private Teacher() { } // EF Core
    private Teacher(Guid id, string fullName, Guid departmentId) : base(id)
    {
        FullName = fullName;
        DepartmentId = departmentId;
    }

    public static Result<Teacher> Create(string fullName, Guid departmentId)
    {
        return string.IsNullOrWhiteSpace(fullName)
            ? Result.Failure<Teacher>(Error.NullValue)
            : new Teacher(Guid.NewGuid(), fullName.Trim(), departmentId);
    }

    public Result UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure(Error.NullValue);
        }

        FullName = fullName.Trim();
        return Result.Success();
    }

    public void ChangeDepartment(Guid departmentId)
    {
        departmentId.ThrowIfNull();
        DepartmentId = departmentId;
    }
}
