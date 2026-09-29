using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RubiksCube.Infrastructure.Persistence;

namespace RubiksCube.Api.Tests;

/// <summary>The real API in-process, on in-memory SQLite. The connection stays open for the factory's lifetime.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public ApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Rubiks", "Data Source=:memory:");
        builder.ConfigureServices(services =>
        {
            // AddDbContext registers its options as a configuration callback, so remove that as well.
            services.RemoveAll<DbContextOptions<RubiksDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RubiksDbContext>>();
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
