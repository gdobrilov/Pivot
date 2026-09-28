using RubiksCube.Domain;

namespace RubiksCube.Application.Snapshots;

/// <summary>Read model of a session: everything a client needs to draw the cube and its controls.</summary>
/// <param name="Id">Session identifier.</param>
/// <param name="IsSolved">True when every face is a single colour.</param>
/// <param name="Version">Optimistic concurrency version; increases with every change.</param>
/// <param name="CanUndo">True when there is an effective move to undo.</param>
/// <param name="EffectiveMoves">Moves that currently shape the cube, in notation.</param>
/// <param name="Faces">The stickers of each face, row by row.</param>
public sealed record CubeSnapshot(
    Guid Id,
    bool IsSolved,
    int Version,
    bool CanUndo,
    IReadOnlyList<string> EffectiveMoves,
    FacesSnapshot Faces);

public sealed record FacesSnapshot(
    IReadOnlyList<Colour> Up,
    IReadOnlyList<Colour> Left,
    IReadOnlyList<Colour> Front,
    IReadOnlyList<Colour> Right,
    IReadOnlyList<Colour> Back,
    IReadOnlyList<Colour> Down);

/// <summary>What a rotation would do, without doing it.</summary>
/// <param name="Move">The move in notation, e.g. <c>R'</c>.</param>
/// <param name="After">The cube as it would look afterwards.</param>
/// <param name="Changes">Every sticker whose colour would change.</param>
public sealed record RotationPreview(string Move, FacesSnapshot After, IReadOnlyList<StickerChange> Changes);

public sealed record StickerChange(Face Face, int Row, int Column, Colour From, Colour To);

/// <summary>One line of the session's audit log.</summary>
public sealed record RotationLogEntrySnapshot(
    int Sequence,
    string Kind,
    Face? Face,
    Rotation? Rotation,
    string? Move,
    DateTimeOffset OccurredAtUtc);
