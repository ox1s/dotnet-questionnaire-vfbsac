using SharedKernel;

namespace Domain.Questionnaires.FormAggregate.Events;

public sealed record FormCreatedDomainEvent(Guid FormId) : IDomainEvent;
