namespace RubiksCube.Domain;

/// <summary>One of the four sides of a face, as seen when looking straight at that face in its net orientation.</summary>
public enum Side
{
    Top,
    Right,
    Bottom,
    Left,
}

/// <summary>One of the four corners of a face, in the same orientation as <see cref="Side"/>.</summary>
public enum Corner
{
    TopLeft,
    TopRight,
    BottomRight,
    BottomLeft,
}
