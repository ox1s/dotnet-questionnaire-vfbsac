namespace Application.Users.GetById;

public sealed record GetUserByIdQueryResponse
{
    public Guid Id { get; init; }
    public string Login { get; init; }
    public string DisplayName { get; init; }
}
