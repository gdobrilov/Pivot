using Microsoft.EntityFrameworkCore;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Infrastructure.Persistence;

public sealed class EfCubeSessionRepository(RubiksDbContext dbContext) : ICubeSessionRepository
{
    public Task<CubeSession?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Sessions.SingleOrDefaultAsync(session => session.Id == id, cancellationToken);

    public async Task AddAsync(CubeSession session, CancellationToken cancellationToken = default) =>
        await dbContext.Sessions.AddAsync(session, cancellationToken).ConfigureAwait(false);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException("The session was modified by another request.", exception);
        }
    }
}
