using RubiksCube.Domain;

namespace RubiksCube.Application.Snapshots;

public sealed record RotationLogEntrySnapshot(
    int Sequence,
    string Kind,
    Face? Face,
    Rotation? Rotation,
    string? Move,
    DateTimeOffset OccurredAtUtc);
