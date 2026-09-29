namespace RubiksCube.Domain.Sessions;

/// <summary>One line of the log. For an undo, the move is the one that was undone; a reset has none.</summary>
public sealed record RotationLogEntry(int Sequence, LogEntryKind Kind, Face? Face, Rotation? Rotation, DateTimeOffset OccurredAtUtc)
{
    public Move? Move => Face is { } face && Rotation is { } rotation ? new Move(face, rotation) : null;
}
