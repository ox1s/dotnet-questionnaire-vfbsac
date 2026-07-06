using SharedKernel;
using Throw;

namespace Domain.College.Disciplines;

public sealed class Discipline : Entity, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; set; }
    public Guid DepartmentId { get; private set; }

    private Discipline() { } // EF Core
    private Discipline(Guid id, string name, Guid departmentId) : base(id)
    {
        Name = name;
        DepartmentId = departmentId;
    }

    public static Result<Discipline> Create(string name, Guid departmentId)
    {
        return string.IsNullOrWhiteSpace(name) ? 
            Result.Failure<Discipline>(Error.NullValue) 
            : new Discipline(Guid.NewGuid(), name.Trim(), departmentId);
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.NullValue);
        }

        Name = name.Trim();
        return Result.Success();
    }

    public void ChangeDepartment(Guid departmentId)
    {
        departmentId.ThrowIfNull();
        DepartmentId = departmentId;
    }
}
