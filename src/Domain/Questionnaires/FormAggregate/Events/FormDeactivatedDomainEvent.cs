using SharedKernel;

namespace Domain.Questionnaires.FormAggregate.Events;

public sealed record FormDeactivatedDomainEvent(Guid FormId) : IDomainEvent;
