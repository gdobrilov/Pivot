using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RubiksCube.Infrastructure.Persistence;

/// <summary>For <c>dotnet ef migrations add</c>, so it does not need the API.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RubiksDbContext>
{
    public RubiksDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<RubiksDbContext>().UseSqlite("Data Source=design-time.db").Options);
}
