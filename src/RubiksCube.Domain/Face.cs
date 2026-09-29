namespace RubiksCube.Domain;

/// <summary>A face is a position in space, not a colour: we never turn the whole cube, so Up stays Up.</summary>
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
    /// <summary>All faces in net order: U, L, F, R, B, D.</summary>
    public static IReadOnlyList<Face> All { get; } = Enum.GetValues<Face>();
}
