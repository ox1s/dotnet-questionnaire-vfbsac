using SharedKernel;

namespace Domain.User;

public sealed record Login(string Value)
{
    public static Result<Login> Create(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Result.Failure<Login>(Error.NullValue)
            : Result.Success(new Login(value.Trim()));
    }
}
