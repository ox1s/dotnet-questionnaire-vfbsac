using SharedKernel;

namespace Domain.College.SpecialityAggregate;

public sealed class Speciality : AggregateRoot, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; set; }
    private Speciality() { }

    private Speciality(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Result<Speciality> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Speciality>(Error.NullValue);
        }

        return new Speciality(Guid.NewGuid(), name.Trim());
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
