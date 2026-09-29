using RubiksCube.Domain;

namespace RubiksCube.Api.Contracts;

/// <summary>What the user picks on screen: a face and how far to turn it.</summary>
public sealed record RotateRequest(Face Face, Rotation Rotation);
