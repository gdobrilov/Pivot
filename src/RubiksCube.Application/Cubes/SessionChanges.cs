using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Common;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Cubes;

/// <summary>Save first, then publish. An outbox would slot in here if events ever left the process.</summary>
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
            return ResultError.Conflict($"Cube session '{session.Id}' was changed by another request at the same time. Try again.");
        }

        // The change is saved; a client that has gone away should not stop its events going out.
        await dispatcher.DispatchAsync(session.DequeueEvents(), CancellationToken.None).ConfigureAwait(false);
        return session.ToSnapshot();
    }
}
