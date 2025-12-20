using SharedKernel;

namespace Domain.College.SpecializationAggregate;

public sealed class Specialization : AggregateRoot
{
    public string? Name { get; private set; }
    public Guid? SpecialityId { get; private set; }

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

    public void UpdateName(string? name)
    {
        Name = name?.Trim();
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
