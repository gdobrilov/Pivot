using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

public sealed class GetCubeHandler(ICubeSessionRepository repository)
    : IQueryHandler<GetCubeQuery, Result<CubeSnapshot>>
{
    public async Task<Result<CubeSnapshot>> HandleAsync(GetCubeQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var session = await repository.FindAsync(query.SessionId, cancellationToken).ConfigureAwait(false);
        return session is null ? ResultError.SessionNotFound(query.SessionId) : session.ToSnapshot();
    }
}

/// <summary>Shows what a move would do. Pure: nothing is stored and no event is raised.</summary>
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

public sealed class GetRotationLogHandler(ICubeSessionRepository repository)
    : IQueryHandler<GetRotationLogQuery, Result<IReadOnlyList<RotationLogEntrySnapshot>>>
{
    public async Task<Result<IReadOnlyList<RotationLogEntrySnapshot>>> HandleAsync(GetRotationLogQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var session = await repository.FindAsync(query.SessionId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return ResultError.SessionNotFound(query.SessionId);
        }

        return Result.Success<IReadOnlyList<RotationLogEntrySnapshot>>(session.Log.Select(entry => entry.ToSnapshot()).ToList());
    }
}
