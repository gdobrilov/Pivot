using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Domain;
using RubiksCube.Domain.Events;

namespace RubiksCube.Application.Tests;

public class CommandHandlerTests
{
    private readonly FakeRepository _repository = new();
    private readonly FakeClock _clock = new();
    private readonly RecordingDispatcher _dispatcher = new();

    [Fact]
    public async Task Create_stores_a_solved_session()
    {
        var snapshot = await new CreateCubeHandler(_repository, _clock).HandleAsync(new CreateCubeCommand());

        Assert.True(snapshot.IsSolved);
        Assert.Empty(snapshot.EffectiveMoves);
        Assert.False(snapshot.CanUndo);
        Assert.Equal(1, _repository.SaveCount);
        Assert.NotNull(await _repository.FindAsync(snapshot.Id));
    }

    [Fact]
    public async Task Rotate_turns_the_face_saves_and_dispatches_the_event_after_saving()
    {
        var id = await CreateAsync();
        _clock.UtcNow = _clock.UtcNow.AddSeconds(3);

        var result = await new RotateFaceHandler(_repository, _clock, _dispatcher)
            .HandleAsync(new RotateFaceCommand(id, Face.Front, Rotation.Clockwise));

        Assert.True(result.IsSuccess);
        Assert.Equal(["F"], result.Value.EffectiveMoves);
        Assert.True(result.Value.CanUndo);
        SnapshotTestExtensions.AssertSameFaces(Cube.Solved().Turn(Move.Clockwise(Face.Front)).ToFacesSnapshotForTest(), result.Value.Faces);
        Assert.Equal(2, _repository.SaveCount);
        var raised = Assert.IsType<CubeRotated>(Assert.Single(_dispatcher.Dispatched));
        Assert.Equal(Move.Clockwise(Face.Front), raised.Move);
        Assert.Equal(_clock.UtcNow, raised.OccurredAtUtc);
    }

    [Fact]
    public async Task Rotate_returns_not_found_for_an_unknown_session()
    {
        var result = await new RotateFaceHandler(_repository, _clock, _dispatcher)
            .HandleAsync(new RotateFaceCommand(Guid.NewGuid(), Face.Front, Rotation.Clockwise));

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Empty(_dispatcher.Dispatched);
    }

    [Fact]
    public async Task Rotate_rejects_values_outside_the_enums_before_touching_the_session()
    {
        var id = await CreateAsync();

        var result = await new RotateFaceHandler(_repository, _clock, _dispatcher)
            .HandleAsync(new RotateFaceCommand(id, (Face)42, Rotation.Clockwise));

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Rotate_reports_a_conflict_when_the_save_races_and_does_not_dispatch()
    {
        var id = await CreateAsync();
        _repository.FailNextSaveWithConflict = true;

        var result = await new RotateFaceHandler(_repository, _clock, _dispatcher)
            .HandleAsync(new RotateFaceCommand(id, Face.Up, Rotation.Half));

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(_dispatcher.Dispatched);
    }

    [Fact]
    public async Task Undo_reverses_the_last_move_and_refuses_when_there_is_nothing_to_undo()
    {
        var id = await CreateAsync();
        await new RotateFaceHandler(_repository, _clock, _dispatcher).HandleAsync(new RotateFaceCommand(id, Face.Left, Rotation.AntiClockwise));
        var undo = new UndoRotationHandler(_repository, _clock, _dispatcher);

        var first = await undo.HandleAsync(new UndoRotationCommand(id));
        var second = await undo.HandleAsync(new UndoRotationCommand(id));

        Assert.True(first.IsSuccess);
        Assert.True(first.Value.IsSolved);
        Assert.False(first.Value.CanUndo);
        Assert.Equal(ErrorType.Validation, second.Error.Type);
        Assert.IsType<RotationUndone>(_dispatcher.Dispatched[^1]);
    }

    [Fact]
    public async Task Reset_solves_the_cube_and_bumps_the_version()
    {
        var id = await CreateAsync();
        await new RotateFaceHandler(_repository, _clock, _dispatcher).HandleAsync(new RotateFaceCommand(id, Face.Down, Rotation.Clockwise));

        var result = await new ResetCubeHandler(_repository, _clock, _dispatcher).HandleAsync(new ResetCubeCommand(id));

        Assert.True(result.Value.IsSolved);
        Assert.Equal(2, result.Value.Version);
        Assert.IsType<CubeReset>(_dispatcher.Dispatched[^1]);
    }

    private async Task<Guid> CreateAsync() =>
        (await new CreateCubeHandler(_repository, _clock).HandleAsync(new CreateCubeCommand())).Id;
}
