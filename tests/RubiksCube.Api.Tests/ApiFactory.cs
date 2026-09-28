using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RubiksCube.Infrastructure.Persistence;

namespace RubiksCube.Api.Tests;

/// <summary>
/// Hosts the real API in-process against a private in-memory SQLite database. The connection is
/// kept open for the factory's lifetime because an in-memory SQLite database lives only as long
/// as its connection. Migrations run at startup exactly as in production.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RubiksDbContext>>();
            services.AddDbContext<RubiksDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
