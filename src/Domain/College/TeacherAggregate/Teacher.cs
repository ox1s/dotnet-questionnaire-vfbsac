using SharedKernel;
using Throw;

namespace Domain.College.TeacherAggregate;

public sealed class Teacher : AggregateRoot, ISoftDeletable
{
    public string FullName { get; private set; }
    public Guid DepartmentId { get; private set; }
    public bool IsDeleted { get; set; }

    private Teacher() { } // EF Core
    private Teacher(Guid id, string fullName, Guid departmentId) : base(id)
    {
        FullName = fullName;
        DepartmentId = departmentId;
    }

    public static Result<Teacher> Create(string fullName, Guid departmentId)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<Teacher>(Error.NullValue);
        }

        return new Teacher(Guid.NewGuid(), fullName.Trim(), departmentId);
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
