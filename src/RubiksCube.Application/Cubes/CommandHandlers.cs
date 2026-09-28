using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Cubes;

public sealed class CreateCubeHandler(ICubeSessionRepository repository, IClock clock)
    : ICommandHandler<CreateCubeCommand, CubeSnapshot>
{
    public async Task<CubeSnapshot> HandleAsync(CreateCubeCommand command, CancellationToken cancellationToken = default)
    {
        var session = CubeSession.Create(clock.UtcNow);
        await repository.AddAsync(session, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return session.ToSnapshot();
    }
}

public sealed class RotateFaceHandler(ICubeSessionRepository repository, IClock clock, IDomainEventDispatcher dispatcher)
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

        session.Rotate(new Move(command.Face, command.Rotation), clock.UtcNow);
        return await SessionChanges.CommitAsync(session, repository, dispatcher, cancellationToken).ConfigureAwait(false);
    }
}

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

/// <summary>
/// The one place that knows the order: persist first, then tell the world. If a message bus is
/// ever introduced, this is where an outbox would replace the direct dispatch.
/// </summary>
internal static class SessionChanges
{
    public static async Task<Result<CubeSnapshot>> CommitAsync(
        CubeSession session,
        ICubeSessionRepository repository,
        IDomainEventDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ConcurrencyConflictException)
        {
            return ResultError.Conflict($"Cube session '{session.Id}' was changed by another request. Reload and try again.");
        }

        await dispatcher.DispatchAsync(session.DequeueEvents(), cancellationToken).ConfigureAwait(false);
        return session.ToSnapshot();
    }
}
