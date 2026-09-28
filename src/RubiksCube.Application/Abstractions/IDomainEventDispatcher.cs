using RubiksCube.Domain.Events;

namespace RubiksCube.Application.Abstractions;

/// <summary>
/// Hands domain events to whoever is interested, after the change that produced them has been
/// saved. The in-process implementation calls handlers synchronously; a message-bus
/// implementation would publish instead, without the application layer changing.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default);
}

public interface IDomainEventListener<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
