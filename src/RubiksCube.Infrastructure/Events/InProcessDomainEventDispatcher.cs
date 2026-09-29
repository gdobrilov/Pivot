using Microsoft.Extensions.DependencyInjection;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain.Events;

namespace RubiksCube.Infrastructure.Events;

/// <summary>Calls the listeners for each event, in order, on the calling thread. No queue, no retry.</summary>
public sealed class InProcessDomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);
        foreach (var domainEvent in events)
        {
            // Listeners are typed by event, so look them up by the event's runtime type.
            var listenerType = typeof(IDomainEventListener<>).MakeGenericType(domainEvent.GetType());
            var handle = listenerType.GetMethod(nameof(IDomainEventListener<IDomainEvent>.HandleAsync))!;
            foreach (var listener in serviceProvider.GetServices(listenerType))
            {
                await ((Task)handle.Invoke(listener, [domainEvent, cancellationToken])!).ConfigureAwait(false);
            }
        }
    }
}
