using SharedKernel;

namespace Domain.College.DisciplineAggregate;

public sealed class Discipline : AggregateRoot
{
    public string Name { get; private set; }
    public Guid DepartmentId { get; private set; }

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

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        Name = name.Trim();
    }

    public void ChangeDepartment(Guid departmentId)
    {
        if (departmentId != Guid.Empty)
        {
            DepartmentId = departmentId;
        }
    }
}
