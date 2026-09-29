using RubiksCube.Application.Abstractions;
using RubiksCube.Domain.Events;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Tests;

/// <summary>Test doubles, so the handlers run without any infrastructure.</summary>
internal sealed class FakeRepository : ICubeSessionRepository
{
    private readonly Dictionary<Guid, CubeSession> _sessions = [];

    public int SaveCount { get; private set; }

    public bool FailNextSaveWithConflict { get; set; }

    public Task<CubeSession?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessions.GetValueOrDefault(id));

    public Task AddAsync(CubeSession session, CancellationToken cancellationToken = default)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (FailNextSaveWithConflict)
        {
            FailNextSaveWithConflict = false;
            throw new ConcurrencyConflictException("simulated");
        }

        SaveCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);
}

internal sealed class RecordingDispatcher : IDomainEventDispatcher
{
    public List<IDomainEvent> Dispatched { get; } = [];

    public Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        Dispatched.AddRange(events);
        return Task.CompletedTask;
    }
}
