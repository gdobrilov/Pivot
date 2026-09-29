using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Abstractions;

/// <summary>Load a session, change it, call <see cref="SaveChangesAsync"/>. Implemented in Infrastructure.</summary>
public interface ICubeSessionRepository
{
    Task<CubeSession?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(CubeSession session, CancellationToken cancellationToken = default);

    /// <exception cref="ConcurrencyConflictException">Someone else changed the session in the meantime.</exception>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
