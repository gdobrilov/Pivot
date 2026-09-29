namespace RubiksCube.Domain;

public static class RotationExtensions
{
    public static int QuarterTurns(this Rotation rotation) => rotation switch
    {
        Rotation.Clockwise => 1,
        Rotation.Half => 2,
        Rotation.AntiClockwise => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Unknown rotation."),
    };

    public static Rotation Inverse(this Rotation rotation) => rotation switch
    {
        Rotation.Clockwise => Rotation.AntiClockwise,
        Rotation.AntiClockwise => Rotation.Clockwise,
        Rotation.Half => Rotation.Half,
        _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Unknown rotation."),
    };
}
