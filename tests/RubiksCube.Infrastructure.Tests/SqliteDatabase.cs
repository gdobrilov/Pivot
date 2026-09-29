using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RubiksCube.Infrastructure.Persistence;

namespace RubiksCube.Infrastructure.Tests;

/// <summary>In-memory SQLite per test, created by the real migration. The connection stays open or the database is gone.</summary>
internal sealed class SqliteDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteDatabase()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.Migrate();
    }

    public RubiksDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<RubiksDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
