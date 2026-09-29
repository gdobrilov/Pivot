using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application.Cubes;

public sealed class ResetCubeHandler(ICubeSessionRepository repository, IClock clock, IDomainEventDispatcher dispatcher)
    : ICommandHandler<ResetCubeCommand, Result<CubeSnapshot>>
{
    public async Task<Result<CubeSnapshot>> HandleAsync(ResetCubeCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var session = await repository.FindAsync(command.SessionId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return ResultError.SessionNotFound(command.SessionId);
        }

        session.Reset(clock.UtcNow);
        return await SessionChanges.CommitAsync(session, repository, dispatcher, cancellationToken).ConfigureAwait(false);
    }
}
