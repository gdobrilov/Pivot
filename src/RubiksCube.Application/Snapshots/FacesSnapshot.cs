using RubiksCube.Domain;

namespace RubiksCube.Application.Snapshots;

/// <summary>Nine colours per face, row by row.</summary>
public sealed record FacesSnapshot(
    IReadOnlyList<Colour> Up,
    IReadOnlyList<Colour> Left,
    IReadOnlyList<Colour> Front,
    IReadOnlyList<Colour> Right,
    IReadOnlyList<Colour> Back,
    IReadOnlyList<Colour> Down);
