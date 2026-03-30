using SharedKernel;

namespace Domain.College.Speciality;

public sealed class Speciality : Entity, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; set; }

    private Speciality() { } // EF Core
    private Speciality(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Result<Speciality> Create(string name)
    {
        return string.IsNullOrWhiteSpace(name) ?
            Result.Failure<Speciality>(Error.NullValue) 
            : new Speciality(Guid.NewGuid(), name.Trim());
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
