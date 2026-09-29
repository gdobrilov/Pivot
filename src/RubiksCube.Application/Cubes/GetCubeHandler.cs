using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

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
