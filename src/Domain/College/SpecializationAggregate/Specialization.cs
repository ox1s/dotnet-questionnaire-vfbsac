using SharedKernel;

namespace Domain.College.SpecializationAggregate;

public sealed class Specialization : AggregateRoot, ISoftDeletable
{
    public string? Name { get; private set; }
    public Guid? SpecialityId { get; private set; }
    public bool IsDeleted { get; set; }
    private Specialization() { }

    private Specialization(Guid id, string? name, Guid? specialityId) : base(id)
    {
        Name = name;
        SpecialityId = specialityId;
    }

    public static Result<Specialization> Create(string? name = null, Guid? specialityId = null)
    {
        return new Specialization(Guid.NewGuid(), name?.Trim(), specialityId);
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

    public void AssignToSpeciality(Guid specialityId)
    {
        SpecialityId = specialityId;
    }

    public void RemoveFromSpeciality()
    {
        SpecialityId = null;
    }
}
