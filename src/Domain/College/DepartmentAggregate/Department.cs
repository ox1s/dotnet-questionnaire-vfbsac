using SharedKernel;

namespace Domain.College.DepartmentAggregate;

public sealed class Department : AggregateRoot, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; set; }
    private Department() { }

    private Department(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Result<Department> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Department>(Error.NullValue);
        }

        return new Department(Guid.NewGuid(), name.Trim());
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
}
