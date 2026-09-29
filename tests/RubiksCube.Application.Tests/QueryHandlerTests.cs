using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Application.Rendering;
using RubiksCube.Domain;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Tests;

public class QueryHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Get_returns_the_current_state()
    {
        var id = await CreateAsync();

        var result = await new GetCubeHandler(Repository).HandleAsync(new GetCubeQuery(id));

        Assert.True(result.Value.IsSolved);
    }

    [Fact]
    public async Task Get_returns_not_found_for_an_unknown_session()
    {
        var result = await new GetCubeHandler(Repository).HandleAsync(new GetCubeQuery(Guid.NewGuid()));

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Preview_shows_the_result_and_the_changed_stickers_without_changing_anything()
    {
        var id = await CreateAsync();

        var result = await new PreviewRotationHandler(Repository).HandleAsync(new PreviewRotationQuery(id, Face.Front, Rotation.Clockwise));

        Assert.Equal("F", result.Value.Move);
        AssertSameFaces(Cube.Solved().Turn(Move.Clockwise(Face.Front)), result.Value.After);
        // On a solved cube the front face keeps its colour, so only the 12 strip stickers change.
        Assert.Equal(12, result.Value.Changes.Count);
        Assert.Contains(result.Value.Changes, change => change is { Face: Face.Right, Row: 0, Column: 0, From: Colour.Red, To: Colour.White });
        Assert.Empty(Dispatcher.Dispatched);
        Assert.True((await new GetCubeHandler(Repository).HandleAsync(new GetCubeQuery(id))).Value.IsSolved);
    }

    [Fact]
    public async Task Preview_rejects_values_outside_the_enums()
    {
        var id = await CreateAsync();

        var result = await new PreviewRotationHandler(Repository).HandleAsync(new PreviewRotationQuery(id, Face.Front, (Rotation)9));

        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task Log_lists_everything_that_happened_in_order_with_timestamps()
    {
        var id = await CreateAsync();
        await RotateAsync(id, Face.Front, Rotation.Clockwise);
        Time.Now = Time.Now.AddMinutes(1);
        await new UndoRotationHandler(Repository, Time, Dispatcher).HandleAsync(new UndoRotationCommand(id));
        await new ResetCubeHandler(Repository, Time, Dispatcher).HandleAsync(new ResetCubeCommand(id));

        var log = (await new GetRotationLogHandler(Repository).HandleAsync(new GetRotationLogQuery(id))).Value;

        Assert.Equal([1, 2, 3], log.Select(entry => entry.Sequence));
        Assert.Equal([LogEntryKind.Rotation, LogEntryKind.Undo, LogEntryKind.Reset], log.Select(entry => entry.Kind));
        Assert.Equal(["F", "F", null], log.Select(entry => entry.Move));
        Assert.Equal(Time.Now, log[1].OccurredAtUtc);
    }

    [Fact]
    public async Task Net_is_the_rendered_exploded_view()
    {
        var id = await CreateAsync();

        var result = await new GetNetHandler(Repository, new ExplodedViewRenderer()).HandleAsync(new GetNetQuery(id));

        Assert.StartsWith("       W W W\n", result.Value, StringComparison.Ordinal);
    }
}
