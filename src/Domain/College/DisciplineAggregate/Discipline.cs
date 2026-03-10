using SharedKernel;

namespace Domain.College.DisciplineAggregate;

public sealed class Discipline : AggregateRoot, ISoftDeletable
{
    public string Name { get; private set; }
    public Guid DepartmentId { get; private set; }
    public bool IsDeleted { get; set; }
    private Discipline() { }

    private Discipline(Guid id, string name, Guid departmentId) : base(id)
    {
        Name = name;
        DepartmentId = departmentId;
    }

    public static Result<Discipline> Create(string name, Guid departmentId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Discipline>(Error.NullValue);
        }

        return new Discipline(Guid.NewGuid(), name.Trim(), departmentId);
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
        if (departmentId != Guid.Empty)
        {
            DepartmentId = departmentId;
        }
    }
}
