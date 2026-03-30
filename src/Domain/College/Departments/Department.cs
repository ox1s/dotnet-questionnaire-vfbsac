using SharedKernel;

namespace Domain.College.Departments;

public sealed class Department : Entity, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; set; }

    private Department() { } // EF Core
    private Department(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Result<Department> Create(string name)
    {
        return string.IsNullOrWhiteSpace(name) ?
            Result.Failure<Department>(Error.NullValue)
            : new Department(Guid.NewGuid(), name.Trim());
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
