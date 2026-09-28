namespace RubiksCube.Domain;

/// <summary>How far a face is turned, as seen when looking straight at that face.</summary>
public enum Rotation
{
    /// <summary>90° clockwise.</summary>
    Clockwise,

    /// <summary>90° anti-clockwise.</summary>
    AntiClockwise,

    /// <summary>180°; its own inverse.</summary>
    Half,
}

public static class RotationExtensions
{
    /// <summary>Number of clockwise quarter turns that produce this rotation.</summary>
    public static int QuarterTurns(this Rotation rotation) => rotation switch
    {
        Rotation.Clockwise => 1,
        Rotation.Half => 2,
        Rotation.AntiClockwise => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Unknown rotation."),
    };

    /// <summary>The rotation that undoes this one.</summary>
    public static Rotation Inverse(this Rotation rotation) => rotation switch
    {
        Rotation.Clockwise => Rotation.AntiClockwise,
        Rotation.AntiClockwise => Rotation.Clockwise,
        Rotation.Half => Rotation.Half,
        _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Unknown rotation."),
    };
}
