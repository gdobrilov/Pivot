namespace RubiksCube.Domain;

/// <summary>
/// The six faces of the cube as positions in space, named from the solver's point of view.
/// A face keeps its name forever because whole-cube rotations are not modelled; only the
/// stickers on it change.
/// </summary>
public enum Face
{
    Up,
    Left,
    Front,
    Right,
    Back,
    Down,
}

public static class Faces
{
    /// <summary>All faces in net order (U, L, F, R, B, D).</summary>
    public static IReadOnlyList<Face> All { get; } = Enum.GetValues<Face>();
}
