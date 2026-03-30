using SharedKernel;
using Throw;

namespace Domain.College.Specializations;

public sealed class Specialization : Entity, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; set; }
    public Guid SpecialityId { get; private set; }

    private Specialization() { } // EF Core
    private Specialization(Guid id, string name, Guid specialityId) : base(id)
    {
        Name = name;
        SpecialityId = specialityId;
    }

    public static Result<Specialization> Create(string name, Guid specialityId)
    {
        return new Specialization(Guid.NewGuid(), name.Trim(), specialityId);
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

    public void ChangeSpeciality(Guid specialityId)
    {
        specialityId.ThrowIfNull();
        SpecialityId = specialityId;
    }
}
