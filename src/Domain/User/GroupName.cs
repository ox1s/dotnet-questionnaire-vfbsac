using SharedKernel;

namespace Domain.User;

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
            return Result.Failure<GroupName>(UserErrors.GroupNameInvalid());
        }

        return new GroupName(value.Trim().ToUpperInvariant());
    }
}
