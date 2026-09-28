using RubiksCube.Application.Common;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

internal static class MoveValidation
{
    /// <summary>Guards against values outside the enums, which can arrive from any untyped client.</summary>
    public static ResultError? Validate(Face face, Rotation rotation)
    {
        if (!Enum.IsDefined(face))
        {
            return ResultError.Validation($"'{face}' is not a face. Expected one of {string.Join(", ", Enum.GetNames<Face>())}.");
        }

        if (!Enum.IsDefined(rotation))
        {
            return ResultError.Validation($"'{rotation}' is not a rotation. Expected one of {string.Join(", ", Enum.GetNames<Rotation>())}.");
        }

        return null;
    }
}
