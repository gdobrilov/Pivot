using RubiksCube.Domain.Events;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Domain.Tests;

public class CubeSessionTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_new_session_is_solved_with_an_empty_log()
    {
        var session = CubeSession.Create(T0);

        Assert.True(session.Cube.IsSolved);
        Assert.Empty(session.Log);
        Assert.Empty(session.EffectiveMoves);
        Assert.False(session.CanUndo);
        Assert.Equal(0, session.Version);
        Assert.Equal(T0, session.CreatedAtUtc);
    }

    [Fact]
    public void Rotate_turns_the_cube_appends_to_the_log_and_raises_an_event()
    {
        var session = CubeSession.Create(T0);

        var entry = session.Rotate(Move.AntiClockwise(Face.Right), T0.AddSeconds(5));

        Assert.Equal(Cube.Solved().Turn(Move.AntiClockwise(Face.Right)), session.Cube);
        Assert.Equal(new RotationLogEntry(1, LogEntryKind.Rotation, Face.Right, Rotation.AntiClockwise, T0.AddSeconds(5)), entry);
        Assert.Equal([entry], session.Log);
        Assert.Equal(1, session.Version);
        Assert.Equal([new CubeRotated(session.Id, 1, Move.AntiClockwise(Face.Right), T0.AddSeconds(5))], session.DequeueEvents());
        Assert.Empty(session.DequeueEvents());
    }

    [Fact]
    public void Undo_reverses_the_last_effective_move_without_deleting_history()
    {
        var session = CubeSession.Create(T0);
        session.Rotate(Move.Clockwise(Face.Front), T0);
        session.Rotate(Move.Half(Face.Up), T0);
        session.DequeueEvents();

        var entry = session.Undo(T0.AddSeconds(1));

        Assert.Equal(Cube.Solved().Turn(Move.Clockwise(Face.Front)), session.Cube);
        Assert.Equal(new RotationLogEntry(3, LogEntryKind.Undo, Face.Up, Rotation.Half, T0.AddSeconds(1)), entry);
        Assert.Equal(3, session.Version);
        Assert.Equal([Move.Clockwise(Face.Front)], session.EffectiveMoves);
        Assert.Equal([new RotationUndone(session.Id, 3, Move.Half(Face.Up), T0.AddSeconds(1))], session.DequeueEvents());
    }

    [Fact]
    public void Undo_walks_back_one_move_at_a_time()
    {
        var session = CubeSession.Create(T0);
        session.Rotate(Move.Clockwise(Face.Front), T0);
        session.Rotate(Move.Clockwise(Face.Right), T0);

        session.Undo(T0);
        session.Undo(T0);

        Assert.True(session.Cube.IsSolved);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void Undo_with_nothing_to_undo_throws()
    {
        var session = CubeSession.Create(T0);

        Assert.Throws<InvalidOperationException>(() => session.Undo(T0));
    }

    [Fact]
    public void Reset_solves_the_cube_clears_effective_moves_but_keeps_the_log()
    {
        var session = CubeSession.Create(T0);
        session.Rotate(Move.Clockwise(Face.Front), T0);
        session.Rotate(Move.Clockwise(Face.Left), T0);
        session.DequeueEvents();

        var entry = session.Reset(T0.AddMinutes(1));

        Assert.True(session.Cube.IsSolved);
        Assert.Equal(new RotationLogEntry(3, LogEntryKind.Reset, null, null, T0.AddMinutes(1)), entry);
        Assert.Equal(3, session.Log.Count);
        Assert.Equal(3, session.Version);
        Assert.Empty(session.EffectiveMoves);
        Assert.Equal([new CubeReset(session.Id, 3, T0.AddMinutes(1))], session.DequeueEvents());
    }

    [Fact]
    public void Undo_does_not_go_past_a_reset()
    {
        var session = CubeSession.Create(T0);
        session.Rotate(Move.Clockwise(Face.Front), T0);
        session.Reset(T0);
        session.Rotate(Move.Clockwise(Face.Up), T0);

        session.Undo(T0);

        Assert.True(session.Cube.IsSolved);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void Replaying_the_effective_moves_reproduces_the_cube()
    {
        var session = CubeSession.Create(T0);
        foreach (var move in MoveNotation.Parse("F R' U B' L D'"))
        {
            session.Rotate(move, T0);
        }

        session.Undo(T0);
        session.Rotate(Move.Half(Face.Down), T0);

        Assert.Equal(Cube.Solved().Apply(session.EffectiveMoves), session.Cube);
        Assert.Equal("F R' U B' L D2", MoveNotation.Format(session.EffectiveMoves));
    }

    [Fact]
    public void Times_are_stored_in_utc()
    {
        var session = CubeSession.Create(T0);

        var entry = session.Rotate(Move.Clockwise(Face.Front), new DateTimeOffset(2026, 9, 28, 13, 0, 0, TimeSpan.FromHours(3)));

        Assert.Equal(TimeSpan.Zero, entry.OccurredAtUtc.Offset);
        Assert.Equal(T0, entry.OccurredAtUtc);
    }

    [Fact]
    public void Log_sequence_numbers_are_consecutive_from_one()
    {
        var session = CubeSession.Create(T0);
        session.Rotate(Move.Clockwise(Face.Front), T0);
        session.Undo(T0);
        session.Reset(T0);

        Assert.Equal([1, 2, 3], session.Log.Select(entry => entry.Sequence));
    }
}
