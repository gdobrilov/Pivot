using RubiksCube.Domain;

namespace RubiksCube.Application.Snapshots;

/// <summary>What a client needs to draw the cube and its controls.</summary>
public sealed record CubeSnapshot(
    Guid Id,
    bool IsSolved,
    int Version,
    bool CanUndo,
    IReadOnlyList<string> EffectiveMoves,
    FacesSnapshot Faces);

/// <summary>Nine colours per face, row by row.</summary>
public sealed record FacesSnapshot(
    IReadOnlyList<Colour> Up,
    IReadOnlyList<Colour> Left,
    IReadOnlyList<Colour> Front,
    IReadOnlyList<Colour> Right,
    IReadOnlyList<Colour> Back,
    IReadOnlyList<Colour> Down);

/// <summary>What a move would do, without doing it.</summary>
public sealed record RotationPreview(string Move, FacesSnapshot After, IReadOnlyList<StickerChange> Changes);

public sealed record StickerChange(Face Face, int Row, int Column, Colour From, Colour To);

public sealed record RotationLogEntrySnapshot(
    int Sequence,
    string Kind,
    Face? Face,
    Rotation? Rotation,
    string? Move,
    DateTimeOffset OccurredAtUtc);
