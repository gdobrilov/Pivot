using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Abstractions;

/// <summary>Load a session, change it, call <see cref="SaveChangesAsync"/>. Implemented in Infrastructure.</summary>
public interface ICubeSessionRepository
{
    Task<CubeSession?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(CubeSession session);

    /// <exception cref="ConcurrencyConflictException">Another request changed the session at the same time.</exception>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
