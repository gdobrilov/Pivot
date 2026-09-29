using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Cubes;

public sealed class RotateFaceHandler(ICubeSessionRepository repository, TimeProvider timeProvider, IDomainEventDispatcher dispatcher)
    : ICommandHandler<RotateFaceCommand, Result<CubeSnapshot>>
{
    public async Task<Result<CubeSnapshot>> HandleAsync(RotateFaceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (MoveValidation.Validate(command.Face, command.Rotation) is { } invalid)
        {
            return invalid;
        }

        var session = await repository.FindAsync(command.SessionId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return ResultError.SessionNotFound(command.SessionId);
        }

        session.Rotate(new Move(command.Face, command.Rotation), timeProvider.GetUtcNow());
        return await SessionChanges.CommitAsync(session, repository, dispatcher, cancellationToken).ConfigureAwait(false);
    }
}
