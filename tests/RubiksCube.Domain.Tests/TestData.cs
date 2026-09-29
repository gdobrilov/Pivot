namespace RubiksCube.Domain.Tests;

internal static class TestData
{
    public static TheoryData<Face> AllFaces => new(Faces.All);
}
