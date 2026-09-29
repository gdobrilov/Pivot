using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Rendering;

namespace RubiksCube.Application.Cubes;

public sealed class RenderCubeHandler(ICubeSessionRepository repository, ICubeRenderer renderer)
    : IQueryHandler<RenderCubeQuery, Result<string>>
{
    public async Task<Result<string>> HandleAsync(RenderCubeQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var session = await repository.FindAsync(query.SessionId, cancellationToken).ConfigureAwait(false);
        return session is null ? ResultError.SessionNotFound(query.SessionId) : renderer.Render(session.Cube);
    }
}
