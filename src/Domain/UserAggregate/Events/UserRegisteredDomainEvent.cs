using SharedKernel;

namespace Domain.UserAggregate.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Login,
    string DisplayName) : IDomainEvent;
