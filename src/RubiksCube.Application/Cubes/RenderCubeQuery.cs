using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;

namespace RubiksCube.Application.Cubes;

/// <summary>The cube as the plain-text exploded view.</summary>
public sealed record RenderCubeQuery(Guid SessionId) : IQuery<Result<string>>;
