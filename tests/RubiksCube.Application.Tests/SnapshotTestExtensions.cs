using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Application.Tests;

internal static class SnapshotTestExtensions
{
    public static FacesSnapshot ToFacesSnapshotForTest(this Cube cube) => cube.ToFacesSnapshot();

    /// <summary>Records compare lists by reference, so compare face by face.</summary>
    public static void AssertSameFaces(FacesSnapshot expected, FacesSnapshot actual)
    {
        Assert.Equal(expected.Up, actual.Up);
        Assert.Equal(expected.Left, actual.Left);
        Assert.Equal(expected.Front, actual.Front);
        Assert.Equal(expected.Right, actual.Right);
        Assert.Equal(expected.Back, actual.Back);
        Assert.Equal(expected.Down, actual.Down);
    }
}
