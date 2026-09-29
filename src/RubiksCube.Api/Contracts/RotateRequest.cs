using System.ComponentModel.DataAnnotations;
using RubiksCube.Domain;

namespace RubiksCube.Api.Contracts;

/// <summary>What the user picks on screen: a face and how far to turn it. Both are required.</summary>
public sealed record RotateRequest([Required] Face? Face, [Required] Rotation? Rotation);
