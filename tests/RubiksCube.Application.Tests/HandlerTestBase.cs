using RubiksCube.Application.Cubes;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Tests;

public abstract class HandlerTestBase
{
    internal HandlerTestBase()
    {
        Dispatcher = new RecordingDispatcher(Repository);
    }

    internal FakeRepository Repository { get; } = new();

    internal FixedTimeProvider Time { get; } = new();

    internal RecordingDispatcher Dispatcher { get; }

    internal async Task<Guid> CreateAsync() =>
        (await new CreateCubeHandler(Repository, Time).HandleAsync(new CreateCubeCommand())).Id;

    internal Task RotateAsync(Guid id, Face face, Rotation rotation) =>
        new RotateFaceHandler(Repository, Time, Dispatcher).HandleAsync(new RotateFaceCommand(id, face, rotation));

    /// <summary>Records compare lists by reference, so compare face by face.</summary>
    internal static void AssertSameFaces(Cube expected, FacesSnapshot actual)
    {
        var faces = expected.ToFacesSnapshot();
        Assert.Equal(faces.Up, actual.Up);
        Assert.Equal(faces.Left, actual.Left);
        Assert.Equal(faces.Front, actual.Front);
        Assert.Equal(faces.Right, actual.Right);
        Assert.Equal(faces.Back, actual.Back);
        Assert.Equal(faces.Down, actual.Down);
    }
}
