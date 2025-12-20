using SharedKernel;

namespace Domain.UserAggregate.Events;

public sealed record PasswordResetRequestedDomainEvent(
    Guid UserId,
    string Login,
    string Token) : IDomainEvent;
