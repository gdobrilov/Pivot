using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Domain;

namespace RubiksCube.Application.Tests;

public class QueryHandlerTests
{
    private readonly FakeRepository _repository = new();
    private readonly FakeClock _clock = new();
    private readonly RecordingDispatcher _dispatcher = new();

    [Fact]
    public async Task Get_returns_the_current_state_or_not_found()
    {
        var id = await CreateAsync();
        var handler = new GetCubeHandler(_repository);

        var found = await handler.HandleAsync(new GetCubeQuery(id));
        var missing = await handler.HandleAsync(new GetCubeQuery(Guid.NewGuid()));

        Assert.True(found.Value.IsSolved);
        Assert.Equal(ErrorType.NotFound, missing.Error.Type);
    }

    [Fact]
    public async Task Preview_shows_the_result_and_the_changed_stickers_without_changing_anything()
    {
        var id = await CreateAsync();

        var result = await new PreviewRotationHandler(_repository).HandleAsync(new PreviewRotationQuery(id, Face.Front, Rotation.Clockwise));

        Assert.True(result.IsSuccess);
        Assert.Equal("F", result.Value.Move);
        SnapshotTestExtensions.AssertSameFaces(Cube.Solved().Turn(Move.Clockwise(Face.Front)).ToFacesSnapshotForTest(), result.Value.After);
        // On a solved cube the front face keeps its colour, so only the 12 strip stickers change.
        Assert.Equal(12, result.Value.Changes.Count);
        Assert.Contains(result.Value.Changes, change => change is { Face: Face.Right, Row: 0, Column: 0, From: Colour.Red, To: Colour.White });
        Assert.Empty(_dispatcher.Dispatched);
        var unchanged = await new GetCubeHandler(_repository).HandleAsync(new GetCubeQuery(id));
        Assert.True(unchanged.Value.IsSolved);
    }

    [Fact]
    public async Task Preview_validates_input_and_session()
    {
        var id = await CreateAsync();
        var handler = new PreviewRotationHandler(_repository);

        var invalid = await handler.HandleAsync(new PreviewRotationQuery(id, Face.Front, (Rotation)9));
        var missing = await handler.HandleAsync(new PreviewRotationQuery(Guid.NewGuid(), Face.Front, Rotation.Half));

        Assert.Equal(ErrorType.Validation, invalid.Error.Type);
        Assert.Equal(ErrorType.NotFound, missing.Error.Type);
    }

    [Fact]
    public async Task Log_lists_everything_that_happened_in_order_with_timestamps()
    {
        var id = await CreateAsync();
        var rotate = new RotateFaceHandler(_repository, _clock, _dispatcher);
        await rotate.HandleAsync(new RotateFaceCommand(id, Face.Front, Rotation.Clockwise));
        _clock.UtcNow = _clock.UtcNow.AddMinutes(1);
        await new UndoRotationHandler(_repository, _clock, _dispatcher).HandleAsync(new UndoRotationCommand(id));
        await new ResetCubeHandler(_repository, _clock, _dispatcher).HandleAsync(new ResetCubeCommand(id));

        var result = await new GetRotationLogHandler(_repository).HandleAsync(new GetRotationLogQuery(id));

        var log = result.Value;
        Assert.Equal([1, 2, 3], log.Select(entry => entry.Sequence));
        Assert.Equal(["Rotation", "Undo", "Reset"], log.Select(entry => entry.Kind));
        Assert.Equal(["F", "F", null], log.Select(entry => entry.Move));
        Assert.Equal(_clock.UtcNow, log[1].OccurredAtUtc);
    }

    private async Task<Guid> CreateAsync() =>
        (await new CreateCubeHandler(_repository, _clock).HandleAsync(new CreateCubeCommand())).Id;
}
