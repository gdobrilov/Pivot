using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RubiksCube.Infrastructure.Persistence;

/// <summary>Lets <c>dotnet ef migrations add</c> run against this project without starting the API.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RubiksDbContext>
{
    public RubiksDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<RubiksDbContext>().UseSqlite("Data Source=design-time.db").Options);
}
