using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;

namespace RubiksCube.Application.Cubes;

/// <summary>The net (the brief's exploded view) as plain text.</summary>
public sealed record GetNetQuery(Guid SessionId) : IQuery<Result<string>>;
