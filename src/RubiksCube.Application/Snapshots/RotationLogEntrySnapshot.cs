using RubiksCube.Domain;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Snapshots;

/// <summary>One log line. <see cref="Move"/> is the same move in notation, for display.</summary>
public sealed record RotationLogEntrySnapshot(
    int Sequence,
    LogEntryKind Kind,
    Face? Face,
    Rotation? Rotation,
    string? Move,
    DateTimeOffset OccurredAtUtc);
