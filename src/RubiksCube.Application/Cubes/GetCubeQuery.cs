using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application.Cubes;

public sealed record GetCubeQuery(Guid SessionId) : IQuery<Result<CubeSnapshot>>;
