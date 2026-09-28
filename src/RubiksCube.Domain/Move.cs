namespace RubiksCube.Domain;

/// <summary>
/// A turn of one face by one <see cref="Rotation"/>. A small immutable value with no identity:
/// two moves of the same face by the same rotation are the same move, which is what makes it
/// usable as a dictionary key for the pre-computed permutations.
/// </summary>
public readonly record struct Move(Face Face, Rotation Rotation)
{
    public static Move Clockwise(Face face) => new(face, Rotation.Clockwise);

    public static Move AntiClockwise(Face face) => new(face, Rotation.AntiClockwise);

    public static Move Half(Face face) => new(face, Rotation.Half);

    /// <summary>The move that undoes this one.</summary>
    public Move Inverse => new(Face, Rotation.Inverse());

    /// <summary>Singmaster notation, e.g. <c>F</c>, <c>R'</c> or <c>U2</c>.</summary>
    public override string ToString() => MoveNotation.Format(this);
}
