using RubiksCube.Domain.Events;

namespace RubiksCube.Domain.Sessions;

/// <summary>
/// Aggregate root: a cube together with the append-only log of everything done to it.
/// The log is the audit trail and the source of undo; the cube is kept alongside it so the
/// current state never has to be replayed.
/// </summary>
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

    /// <summary>Incremented on every change; used as an optimistic concurrency token by persistence.</summary>
    public int Version { get; private set; }

    /// <summary>Everything done to this session, oldest first.</summary>
    public IReadOnlyList<RotationLogEntry> Log => _log;

    /// <summary>The moves that currently shape the cube: rotations since the last reset, minus undone ones.</summary>
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

    /// <summary>Turns a face and records it.</summary>
    public RotationLogEntry Rotate(Move move, DateTimeOffset now)
    {
        Cube = Cube.Turn(move);
        var entry = Append(LogEntryKind.Rotation, move, now);
        _pendingEvents.Add(new CubeRotated(Id, entry.Sequence, move, now));
        return entry;
    }

    /// <summary>Reverses the most recent effective move by applying its inverse.</summary>
    /// <exception cref="InvalidOperationException">There is nothing to undo; check <see cref="CanUndo"/> first.</exception>
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

    /// <summary>Returns the cube to the solved state. The log keeps everything that came before.</summary>
    public RotationLogEntry Reset(DateTimeOffset now)
    {
        Cube = Cube.Solved(Cube.Size);
        var entry = Append(LogEntryKind.Reset, move: null, now);
        _pendingEvents.Add(new CubeReset(Id, entry.Sequence, now));
        return entry;
    }

    /// <summary>Hands over the events raised since the last call and clears them.</summary>
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
