using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

public sealed record CreateCubeCommand : ICommand<CubeSnapshot>;

public sealed record RotateFaceCommand(Guid SessionId, Face Face, Rotation Rotation) : ICommand<Result<CubeSnapshot>>;

public sealed record UndoRotationCommand(Guid SessionId) : ICommand<Result<CubeSnapshot>>;

public sealed record ResetCubeCommand(Guid SessionId) : ICommand<Result<CubeSnapshot>>;
