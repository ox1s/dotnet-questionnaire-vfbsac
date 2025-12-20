using SharedKernel;

namespace Domain.College.TeacherAggregate;

public sealed class Teacher : AggregateRoot
{
    public string FullName { get; private set; }
    public Guid DepartmentId { get; private set; }

    private Teacher() { }

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

    public void UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return;
        }

        FullName = fullName.Trim();
    }

    public void ChangeDepartment(Guid departmentId)
    {
        DepartmentId = departmentId;
    }
}
