using RubiksCube.Domain.Events;

namespace RubiksCube.Application.Abstractions;

/// <summary>Hands events to their listeners after the change has been saved. In-process today; a bus would go here.</summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default);
}

public interface IDomainEventListener<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
