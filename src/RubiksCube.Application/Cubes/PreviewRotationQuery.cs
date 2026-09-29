using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

public sealed record PreviewRotationQuery(Guid SessionId, Face Face, Rotation Rotation) : IQuery<Result<RotationPreview>>;
