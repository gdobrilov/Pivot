using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Abstractions;

/// <summary>
/// Port for loading and storing sessions. The application layer owns this contract; the
/// infrastructure layer implements it. Changes made to a loaded session are written by
/// <see cref="SaveChangesAsync"/>, which fails with <see cref="ConcurrencyConflictException"/>
/// if someone else changed the same session in the meantime.
/// </summary>
public interface ICubeSessionRepository
{
    Task<CubeSession?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(CubeSession session, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Raised when a session was modified by another request between loading and saving it.</summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
    {
    }

    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
