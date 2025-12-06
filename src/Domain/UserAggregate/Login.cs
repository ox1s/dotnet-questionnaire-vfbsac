using SharedKernel;

namespace Domain.Users;

public sealed record Login(string Value);
{
    public static Result<Login> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Login>(Error.NullValue);
        }

        return new Login(value.Trim().ToLowerInvariant());
    }
}
