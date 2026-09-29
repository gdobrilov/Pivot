using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application.Cubes;

public sealed record ResetCubeCommand(Guid SessionId) : ICommand<Result<CubeSnapshot>>;
