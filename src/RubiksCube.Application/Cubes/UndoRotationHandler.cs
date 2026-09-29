using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application.Cubes;

public sealed class UndoRotationHandler(ICubeSessionRepository repository, IClock clock, IDomainEventDispatcher dispatcher)
    : ICommandHandler<UndoRotationCommand, Result<CubeSnapshot>>
{
    public async Task<Result<CubeSnapshot>> HandleAsync(UndoRotationCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var session = await repository.FindAsync(command.SessionId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return ResultError.SessionNotFound(command.SessionId);
        }

        if (!session.CanUndo)
        {
            return ResultError.Validation("There is nothing to undo.");
        }

        session.Undo(clock.UtcNow);
        return await SessionChanges.CommitAsync(session, repository, dispatcher, cancellationToken).ConfigureAwait(false);
    }
}
