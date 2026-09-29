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

    /// <summary>Bumped on every change; persistence uses it as a concurrency token.</summary>
    public int Version { get; private set; }

    public IReadOnlyList<RotationLogEntry> Log => _log;

    /// <summary>Rotations since the last reset, minus the undone ones.</summary>
    public IReadOnlyList<Move> EffectiveMoves
    {
        get
        {
            var stack = new Stack<Move>();
            foreach (var entry in _log)
            {
                switch (entry.Kind)
                {
                    case LogEntryKind.Rotation:
                        stack.Push(entry.Move!.Value);
                        break;
                    case LogEntryKind.Undo:
                        stack.Pop();
                        break;
                    case LogEntryKind.Reset:
                        stack.Clear();
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown log entry kind {entry.Kind}.");
                }
            }

            return stack.Reverse().ToArray();
        }
    }

    public bool CanUndo => EffectiveMoves.Count > 0;

    public static CubeSession Create(DateTimeOffset now) => new(Guid.NewGuid(), Cube.Solved(), now);

    public RotationLogEntry Rotate(Move move, DateTimeOffset now)
    {
        Cube = Cube.Turn(move);
        var entry = Append(LogEntryKind.Rotation, move, now);
        _pendingEvents.Add(new CubeRotated(Id, entry.Sequence, move, now));
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
        _pendingEvents.Add(new RotationUndone(Id, entry.Sequence, undone, now));
        return entry;
    }

    public RotationLogEntry Reset(DateTimeOffset now)
    {
        Cube = Cube.Solved(Cube.Size);
        var entry = Append(LogEntryKind.Reset, move: null, now);
        _pendingEvents.Add(new CubeReset(Id, entry.Sequence, now));
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
        var entry = new RotationLogEntry(_log.Count + 1, kind, move?.Face, move?.Rotation, now);
        _log.Add(entry);
        Version++;
        return entry;
    }
}
