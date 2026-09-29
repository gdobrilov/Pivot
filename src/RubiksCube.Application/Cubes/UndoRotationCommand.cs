using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application.Cubes;

public sealed record UndoRotationCommand(Guid SessionId) : ICommand<Result<CubeSnapshot>>;
