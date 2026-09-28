namespace RubiksCube.Domain.Sessions;

public enum LogEntryKind
{
    Rotation,
    Undo,
    Reset,
}

/// <summary>
/// One line of a session's append-only log. For a rotation it records the move made; for an undo,
/// the move that was undone; for a reset, nothing but the time.
/// </summary>
public sealed record RotationLogEntry(int Sequence, LogEntryKind Kind, Face? Face, Rotation? Rotation, DateTimeOffset OccurredAtUtc)
{
    /// <summary>The move this entry refers to, if any.</summary>
    public Move? Move => Face is { } face && Rotation is { } rotation ? new Move(face, rotation) : null;
}
