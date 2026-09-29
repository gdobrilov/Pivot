using Microsoft.Extensions.DependencyInjection;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain;
using RubiksCube.Domain.Events;
using RubiksCube.Infrastructure.Events;

namespace RubiksCube.Infrastructure.Tests;

public class InProcessDomainEventDispatcherTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 28, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Each_event_goes_to_every_listener_for_its_type_and_only_those()
    {
        var rotations = new Recorder<CubeRotated>();
        var resets = new Recorder<CubeReset>();
        var alsoRotations = new Recorder<CubeRotated>();
        using var provider = new ServiceCollection()
            .AddSingleton<IDomainEventListener<CubeRotated>>(rotations)
            .AddSingleton<IDomainEventListener<CubeRotated>>(alsoRotations)
            .AddSingleton<IDomainEventListener<CubeReset>>(resets)
            .BuildServiceProvider();
        var rotated = new CubeRotated(Guid.NewGuid(), 1, Move.Clockwise(Face.Front), T0);
        var reset = new CubeReset(rotated.SessionId, 2, T0);
        var undone = new RotationUndone(rotated.SessionId, 3, Move.Clockwise(Face.Front), T0);

        await new InProcessDomainEventDispatcher(provider).DispatchAsync([rotated, reset, undone]);

        Assert.Equal([rotated], rotations.Received);
        Assert.Equal([rotated], alsoRotations.Received);
        Assert.Equal([reset], resets.Received);
    }

    private sealed class Recorder<TEvent> : IDomainEventListener<TEvent>
        where TEvent : IDomainEvent
    {
        public List<TEvent> Received { get; } = [];

        public Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Received.Add(domainEvent);
            return Task.CompletedTask;
        }
    }
}
