using SharedKernel;

namespace Domain.College.Teachers;

public sealed class Teacher : Entity, ISoftDeletable
{
    public string FullName { get; private set; }
    public bool IsDeleted { get; set; }

    private Teacher() { } // EF Core
    private Teacher(Guid id, string fullName) : base(id)
    {
        FullName = fullName;
    }

    public static Result<Teacher> Create(string fullName)
    {
        return string.IsNullOrWhiteSpace(fullName)
            ? Result.Failure<Teacher>(Error.NullValue)
            : new Teacher(Guid.NewGuid(), fullName.Trim());
    }

    public Result UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure(Error.NullValue);
        }

        FullName = fullName.Trim();
        return Result.Success();
    }
}
