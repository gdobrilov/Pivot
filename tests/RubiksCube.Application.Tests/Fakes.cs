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

    public void Add(CubeSession session) => _sessions[session.Id] = session;

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

internal sealed class FixedTimeProvider : TimeProvider
{
    public DateTimeOffset Now { get; set; } = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => Now;
}

/// <summary>Records events and how many saves had happened when they were dispatched.</summary>
internal sealed class RecordingDispatcher(FakeRepository repository) : IDomainEventDispatcher
{
    public List<IDomainEvent> Dispatched { get; } = [];

    public List<int> SaveCountAtDispatch { get; } = [];

    public Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        Dispatched.AddRange(events);
        SaveCountAtDispatch.Add(repository.SaveCount);
        return Task.CompletedTask;
    }
}
