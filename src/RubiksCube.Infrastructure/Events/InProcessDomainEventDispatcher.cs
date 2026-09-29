using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain.Events;

namespace RubiksCube.Infrastructure.Events;

/// <summary>Calls the listeners for each event, in order, on the calling thread. No queue, no retry.</summary>
public sealed class InProcessDomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Invoker> Invokers = new();

    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);
        foreach (var domainEvent in events)
        {
            var invoker = Invokers.GetOrAdd(domainEvent.GetType(), CreateInvoker);
            await invoker.InvokeAsync(serviceProvider, domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }

    private static Invoker CreateInvoker(Type eventType) =>
        (Invoker)Activator.CreateInstance(typeof(Invoker<>).MakeGenericType(eventType))!;

    private abstract class Invoker
    {
        public abstract Task InvokeAsync(IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken);
    }

    private sealed class Invoker<TEvent> : Invoker
        where TEvent : IDomainEvent
    {
        public override async Task InvokeAsync(IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            foreach (var listener in serviceProvider.GetServices<IDomainEventListener<TEvent>>())
            {
                await listener.HandleAsync((TEvent)domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
