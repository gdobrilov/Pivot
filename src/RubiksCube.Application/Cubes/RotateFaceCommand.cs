using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

public sealed record RotateFaceCommand(Guid SessionId, Face Face, Rotation Rotation) : ICommand<Result<CubeSnapshot>>;
