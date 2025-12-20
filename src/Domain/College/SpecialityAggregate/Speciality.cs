using SharedKernel;

namespace Domain.College.SpecialityAggregate;

public sealed class Speciality : AggregateRoot
{
    public string Name { get; private set; }

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

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        Name = name.Trim();
    }
}
