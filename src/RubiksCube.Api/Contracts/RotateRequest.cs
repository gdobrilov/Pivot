using RubiksCube.Domain;

namespace RubiksCube.Api.Contracts;

/// <summary>A face to turn and how far: what a user picks on screen.</summary>
/// <param name="Face">Up, Left, Front, Right, Back or Down.</param>
/// <param name="Rotation">Clockwise (90°), AntiClockwise (90°) or Half (180°), as seen looking at the face.</param>
public sealed record RotateRequest(Face Face, Rotation Rotation);
