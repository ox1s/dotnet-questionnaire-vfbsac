using SharedKernel;

namespace Domain.Questionnaires.SubmissionAggregate.Events;

public sealed record SubmissionCreatedDomainEvent(Guid SubmissionId, Guid FormId, Guid UserId) : IDomainEvent;
