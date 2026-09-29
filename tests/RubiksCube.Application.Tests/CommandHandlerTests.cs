using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Domain;
using RubiksCube.Domain.Events;

namespace RubiksCube.Application.Tests;

public class CommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Create_stores_a_solved_session()
    {
        var snapshot = await new CreateCubeHandler(Repository, Time).HandleAsync(new CreateCubeCommand());

        Assert.True(snapshot.IsSolved);
        Assert.Empty(snapshot.EffectiveMoves);
        Assert.False(snapshot.CanUndo);
        Assert.Equal(1, Repository.SaveCount);
        Assert.NotNull(await Repository.FindAsync(snapshot.Id));
    }

    [Fact]
    public async Task Rotate_turns_the_face_and_saves()
    {
        var id = await CreateAsync();

        var result = await new RotateFaceHandler(Repository, Time, Dispatcher)
            .HandleAsync(new RotateFaceCommand(id, Face.Front, Rotation.Clockwise));

        Assert.True(result.IsSuccess);
        Assert.Equal(["F"], result.Value.EffectiveMoves);
        Assert.True(result.Value.CanUndo);
        AssertSameFaces(Cube.Solved().Turn(Move.Clockwise(Face.Front)), result.Value.Faces);
        Assert.Equal(2, Repository.SaveCount);
    }

    [Fact]
    public async Task Rotate_dispatches_the_event_only_after_the_save()
    {
        var id = await CreateAsync();
        Time.Now = Time.Now.AddSeconds(3);

        await RotateAsync(id, Face.Front, Rotation.Clockwise);

        var raised = Assert.IsType<CubeRotated>(Assert.Single(Dispatcher.Dispatched));
        Assert.Equal(Move.Clockwise(Face.Front), raised.Move);
        Assert.Equal(Time.Now, raised.OccurredAtUtc);
        Assert.Equal([2], Dispatcher.SaveCountAtDispatch);
    }

    [Fact]
    public async Task Rotate_returns_not_found_for_an_unknown_session()
    {
        var result = await new RotateFaceHandler(Repository, Time, Dispatcher)
            .HandleAsync(new RotateFaceCommand(Guid.NewGuid(), Face.Front, Rotation.Clockwise));

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Empty(Dispatcher.Dispatched);
    }

    [Fact]
    public async Task Rotate_rejects_values_outside_the_enums_before_touching_the_session()
    {
        var id = await CreateAsync();

        var result = await new RotateFaceHandler(Repository, Time, Dispatcher)
            .HandleAsync(new RotateFaceCommand(id, (Face)42, Rotation.Clockwise));

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(1, Repository.SaveCount);
    }

    [Fact]
    public async Task Rotate_reports_a_conflict_when_the_save_races_and_does_not_dispatch()
    {
        var id = await CreateAsync();
        Repository.FailNextSaveWithConflict = true;

        var result = await new RotateFaceHandler(Repository, Time, Dispatcher)
            .HandleAsync(new RotateFaceCommand(id, Face.Up, Rotation.Half));

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(Dispatcher.Dispatched);
    }

    [Fact]
    public async Task Undo_reverses_the_last_move()
    {
        var id = await CreateAsync();
        await RotateAsync(id, Face.Left, Rotation.AntiClockwise);

        var result = await new UndoRotationHandler(Repository, Time, Dispatcher).HandleAsync(new UndoRotationCommand(id));

        Assert.True(result.Value.IsSolved);
        Assert.False(result.Value.CanUndo);
        Assert.IsType<RotationUndone>(Dispatcher.Dispatched[^1]);
    }

    [Fact]
    public async Task Undo_with_nothing_to_undo_is_a_conflict()
    {
        var id = await CreateAsync();

        var result = await new UndoRotationHandler(Repository, Time, Dispatcher).HandleAsync(new UndoRotationCommand(id));

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Undo_and_reset_return_not_found_for_an_unknown_session()
    {
        var undo = await new UndoRotationHandler(Repository, Time, Dispatcher).HandleAsync(new UndoRotationCommand(Guid.NewGuid()));
        var reset = await new ResetCubeHandler(Repository, Time, Dispatcher).HandleAsync(new ResetCubeCommand(Guid.NewGuid()));

        Assert.Equal(ErrorType.NotFound, undo.Error.Type);
        Assert.Equal(ErrorType.NotFound, reset.Error.Type);
    }

    [Fact]
    public async Task Reset_solves_the_cube_and_bumps_the_version()
    {
        var id = await CreateAsync();
        await RotateAsync(id, Face.Down, Rotation.Clockwise);

        var result = await new ResetCubeHandler(Repository, Time, Dispatcher).HandleAsync(new ResetCubeCommand(id));

        Assert.True(result.Value.IsSolved);
        Assert.Equal(2, result.Value.Version);
        Assert.IsType<CubeReset>(Dispatcher.Dispatched[^1]);
    }
}
