using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application.Cubes;

public sealed record CreateCubeCommand : ICommand<CubeSnapshot>;
