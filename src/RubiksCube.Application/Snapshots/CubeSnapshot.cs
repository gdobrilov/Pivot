namespace RubiksCube.Application.Snapshots;

/// <summary>What a client needs to draw the cube and its controls.</summary>
public sealed record CubeSnapshot(
    Guid Id,
    bool IsSolved,
    int Version,
    bool CanUndo,
    IReadOnlyList<string> EffectiveMoves,
    FacesSnapshot Faces);
