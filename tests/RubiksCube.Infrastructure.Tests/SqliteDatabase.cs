using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RubiksCube.Infrastructure.Persistence;

namespace RubiksCube.Infrastructure.Tests;

/// <summary>
/// A private in-memory SQLite database per test, created through the real migrations. The
/// connection must stay open: an in-memory SQLite database disappears when it is closed.
/// </summary>
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
