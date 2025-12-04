using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Abstractions;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
