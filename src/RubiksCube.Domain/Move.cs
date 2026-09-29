namespace RubiksCube.Domain;

/// <summary>One turn of one face. A value: two equal moves are the same move.</summary>
public readonly record struct Move(Face Face, Rotation Rotation)
{
    public static Move Clockwise(Face face) => new(face, Rotation.Clockwise);

    public static Move AntiClockwise(Face face) => new(face, Rotation.AntiClockwise);

    public static Move Half(Face face) => new(face, Rotation.Half);

    public Move Inverse => new(Face, Rotation.Inverse());

    /// <summary>Notation, e.g. F, R' or U2.</summary>
    public override string ToString() => MoveNotation.Format(this);
}
