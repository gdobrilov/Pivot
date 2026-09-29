namespace RubiksCube.Domain;

/// <summary>
/// A face is a position in space, not a colour: we never turn the whole cube, so Up stays Up.
/// The order is also the order the faces are stored in.
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
