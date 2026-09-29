using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

/// <summary>Nothing is stored and no event is raised.</summary>
public sealed class PreviewRotationHandler(ICubeSessionRepository repository)
    : IQueryHandler<PreviewRotationQuery, Result<RotationPreview>>
{
    public async Task<Result<RotationPreview>> HandleAsync(PreviewRotationQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (MoveValidation.Validate(query.Face, query.Rotation) is { } invalid)
        {
            return invalid;
        }

        var session = await repository.FindAsync(query.SessionId, cancellationToken).ConfigureAwait(false);
        return session is null
            ? ResultError.SessionNotFound(query.SessionId)
            : session.Cube.ToPreview(new Move(query.Face, query.Rotation));
    }
}
