using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application.Cubes;

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
