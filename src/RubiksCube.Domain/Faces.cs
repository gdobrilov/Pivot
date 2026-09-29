namespace RubiksCube.Domain;

public static class Faces
{
    /// <summary>All faces in net order: U, L, F, R, B, D.</summary>
    public static IReadOnlyList<Face> All { get; } = Enum.GetValues<Face>();
}
