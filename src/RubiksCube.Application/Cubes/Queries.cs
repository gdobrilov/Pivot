using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

public sealed record GetCubeQuery(Guid SessionId) : IQuery<Result<CubeSnapshot>>;

public sealed record PreviewRotationQuery(Guid SessionId, Face Face, Rotation Rotation) : IQuery<Result<RotationPreview>>;

public sealed record GetRotationLogQuery(Guid SessionId) : IQuery<Result<IReadOnlyList<RotationLogEntrySnapshot>>>;

/// <summary>The cube as the plain-text exploded view.</summary>
public sealed record RenderCubeQuery(Guid SessionId) : IQuery<Result<string>>;
