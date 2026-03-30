using SharedKernel;

namespace Domain.UserAggregate;

public sealed record GroupName(string Value)
{
    public static Result<GroupName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<GroupName>(Error.NullValue);
        }
        if (value.Length != 5)
        {
            return Result.Failure<GroupName>(Error.Failure("Users.GroupNameInvalid", "Название группы должно быть 5 символов"));
        }

        return new GroupName(value.Trim().ToUpperInvariant());
    }
}
