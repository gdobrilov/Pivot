using Microsoft.EntityFrameworkCore;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain;
using RubiksCube.Domain.Sessions;
using RubiksCube.Infrastructure.Persistence;

namespace RubiksCube.Infrastructure.Tests;

public sealed class EfCubeSessionRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 28, 9, 30, 0, TimeSpan.Zero);
    private readonly SqliteDatabase _database = new();

    [Fact]
    public async Task A_session_and_its_log_round_trip_through_the_database()
    {
        var session = CubeSession.Create(T0);
        session.Rotate(Move.Clockwise(Face.Front), T0.AddSeconds(1));
        session.Rotate(Move.Half(Face.Up), T0.AddSeconds(2));
        session.Undo(T0.AddSeconds(3));
        session.Reset(T0.AddSeconds(4));
        session.Rotate(Move.AntiClockwise(Face.Down), T0.AddSeconds(5));

        await using (var context = _database.CreateContext())
        {
            var repository = new EfCubeSessionRepository(context);
            await repository.AddAsync(session);
            await repository.SaveChangesAsync();
        }

        await using var fresh = _database.CreateContext();
        var loaded = await new EfCubeSessionRepository(fresh).FindAsync(session.Id);

        Assert.NotNull(loaded);
        Assert.Equal(session.Cube, loaded.Cube);
        Assert.Equal(session.CreatedAtUtc, loaded.CreatedAtUtc);
        Assert.Equal(5, loaded.Version);
        Assert.Equal(session.Log, loaded.Log);
        Assert.Equal([Move.AntiClockwise(Face.Down)], loaded.EffectiveMoves);
        Assert.True(loaded.CanUndo);
    }

    [Fact]
    public async Task Find_returns_null_for_an_unknown_id()
    {
        await using var context = _database.CreateContext();

        Assert.Null(await new EfCubeSessionRepository(context).FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Changes_to_a_loaded_session_are_saved_including_new_log_entries()
    {
        var id = await SeedAsync();

        await using (var context = _database.CreateContext())
        {
            var repository = new EfCubeSessionRepository(context);
            var session = (await repository.FindAsync(id))!;
            session.Rotate(Move.Clockwise(Face.Right), T0.AddMinutes(1));
            await repository.SaveChangesAsync();
        }

        await using var fresh = _database.CreateContext();
        var reloaded = (await new EfCubeSessionRepository(fresh).FindAsync(id))!;
        Assert.Equal(["R"], reloaded.EffectiveMoves.Select(move => move.ToString()));
        Assert.Single(reloaded.Log);
    }

    [Fact]
    public async Task Two_requests_changing_the_same_session_conflict_and_the_second_loses()
    {
        var id = await SeedAsync();

        await using var first = _database.CreateContext();
        await using var second = _database.CreateContext();
        var firstRepository = new EfCubeSessionRepository(first);
        var secondRepository = new EfCubeSessionRepository(second);
        var firstSession = (await firstRepository.FindAsync(id))!;
        var secondSession = (await secondRepository.FindAsync(id))!;

        firstSession.Rotate(Move.Clockwise(Face.Front), T0);
        await firstRepository.SaveChangesAsync();
        secondSession.Rotate(Move.Clockwise(Face.Back), T0);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => secondRepository.SaveChangesAsync());

        await using var fresh = _database.CreateContext();
        var stored = (await new EfCubeSessionRepository(fresh).FindAsync(id))!;
        Assert.Equal(["F"], stored.EffectiveMoves.Select(move => move.ToString()));
    }

    [Fact]
    public async Task The_cube_is_stored_as_a_facelet_string_and_the_log_as_readable_names()
    {
        var id = await SeedAsync();
        await using (var context = _database.CreateContext())
        {
            var session = (await context.Sessions.SingleAsync(s => s.Id == id))!;
            session.Rotate(Move.AntiClockwise(Face.Left), T0);
            await context.SaveChangesAsync();
        }

        await using var raw = _database.CreateContext();
        var facelets = await raw.Database.SqlQueryRaw<string>("SELECT Facelets AS Value FROM CubeSessions").SingleAsync();
        var kinds = await raw.Database.SqlQueryRaw<string>("SELECT Kind || ':' || Face || ':' || Rotation AS Value FROM RotationLog").ToListAsync();

        Assert.Equal(Cube.Solved().Turn(Move.AntiClockwise(Face.Left)).ToFacelets(), facelets);
        Assert.Equal(["Rotation:Left:AntiClockwise"], kinds);
    }

    public void Dispose() => _database.Dispose();

    private async Task<Guid> SeedAsync()
    {
        var session = CubeSession.Create(T0);
        await using var context = _database.CreateContext();
        var repository = new EfCubeSessionRepository(context);
        await repository.AddAsync(session);
        await repository.SaveChangesAsync();
        return session.Id;
    }
}
