using Microsoft.EntityFrameworkCore;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Infrastructure.Persistence;

public sealed class RubiksDbContext(DbContextOptions<RubiksDbContext> options) : DbContext(options)
{
    public DbSet<CubeSession> Sessions => Set<CubeSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RubiksDbContext).Assembly);
    }
}
