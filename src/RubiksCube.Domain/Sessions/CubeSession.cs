using RubiksCube.Domain.Events;

namespace RubiksCube.Domain.Sessions;

/// <summary>A cube plus the append-only log of everything done to it. The log is the audit trail and drives undo.</summary>
public sealed class CubeSession
{
    private readonly List<RotationLogEntry> _log = [];
    private readonly List<IDomainEvent> _pendingEvents = [];

    private CubeSession(Guid id, Cube cube, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Cube = cube;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Cube Cube { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>Bumped on every change, so it equals the log length; persistence uses it as the concurrency token.</summary>
    public int Version { get; private set; }

    /// <summary>Oldest first. Sorted here so the order never depends on how the database returns rows.</summary>
    public IReadOnlyList<RotationLogEntry> Log => _log.OrderBy(entry => entry.Sequence).ToList().AsReadOnly();

    /// <summary>Rotations since the last reset, minus the undone ones.</summary>
    public IReadOnlyList<Move> EffectiveMoves
    {
        get
        {
            var stack = new Stack<Move>();
            foreach (var entry in Log)
            {
                switch (entry.Kind)
                {
                    case LogEntryKind.Rotation when entry.Move is { } move:
                        stack.Push(move);
                        break;
                    case LogEntryKind.Undo when stack.Count > 0:
                        stack.Pop();
                        break;
                    case LogEntryKind.Reset:
                        stack.Clear();
                        break;
                    default:
                        throw new InvalidOperationException($"Log entry {entry.Sequence} ({entry.Kind}) does not fit the entries before it.");
                }
            }

            return stack.Reverse().ToArray();
        }
    }

    public bool CanUndo => EffectiveMoves.Count > 0;

    public static CubeSession Create(DateTimeOffset now) => new(Guid.CreateVersion7(now), Cube.Solved(), now.ToUniversalTime());

    public RotationLogEntry Rotate(Move move, DateTimeOffset now)
    {
        Cube = Cube.Turn(move);
        var entry = Append(LogEntryKind.Rotation, move, now);
        _pendingEvents.Add(new CubeRotated(Id, entry.Sequence, move, entry.OccurredAtUtc));
        return entry;
    }

    /// <summary>Applies the inverse of the last effective move. Check <see cref="CanUndo"/> first.</summary>
    public RotationLogEntry Undo(DateTimeOffset now)
    {
        var effective = EffectiveMoves;
        if (effective.Count == 0)
        {
            throw new InvalidOperationException("There is nothing to undo.");
        }

        var undone = effective[^1];
        Cube = Cube.Turn(undone.Inverse);
        var entry = Append(LogEntryKind.Undo, undone, now);
        _pendingEvents.Add(new RotationUndone(Id, entry.Sequence, undone, entry.OccurredAtUtc));
        return entry;
    }

    public RotationLogEntry Reset(DateTimeOffset now)
    {
        Cube = Cube.Solved(Cube.Size);
        var entry = Append(LogEntryKind.Reset, move: null, now);
        _pendingEvents.Add(new CubeReset(Id, entry.Sequence, entry.OccurredAtUtc));
        return entry;
    }

    /// <summary>Returns the events raised since the last call and forgets them.</summary>
    public IReadOnlyList<IDomainEvent> DequeueEvents()
    {
        var events = _pendingEvents.ToArray();
        _pendingEvents.Clear();
        return events;
    }

    private RotationLogEntry Append(LogEntryKind kind, Move? move, DateTimeOffset now)
    {
        var entry = new RotationLogEntry(_log.Count + 1, kind, move?.Face, move?.Rotation, now.ToUniversalTime());
        _log.Add(entry);
        Version++;
        return entry;
    }
}
