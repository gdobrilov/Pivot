namespace RubiksCube.Domain;

/// <summary>A side of a face, as seen looking straight at that face.</summary>
public enum Side
{
    Top,
    Right,
    Bottom,
    Left,
}

/// <summary>A corner of a face, same point of view as <see cref="Side"/>.</summary>
public enum Corner
{
    TopLeft,
    TopRight,
    BottomRight,
    BottomLeft,
}
