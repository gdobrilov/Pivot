using RubiksCube.Application.Rendering;
using RubiksCube.Domain;

namespace RubiksCube.Application.Tests;

public class ExplodedViewRendererTests
{
    private readonly ExplodedViewRenderer _renderer = new();

    [Fact]
    public void Renders_the_solved_cube_as_a_net()
    {
        var expected = Lines(
            "       W W W",
            "       W W W",
            "       W W W",
            "O O O  G G G  R R R  B B B",
            "O O O  G G G  R R R  B B B",
            "O O O  G G G  R R R  B B B",
            "       Y Y Y",
            "       Y Y Y",
            "       Y Y Y");

        Assert.Equal(expected, _renderer.Render(Cube.Solved()));
    }

    [Fact]
    public void Renders_the_challenge_result_exactly_as_in_the_brief()
    {
        var cube = Cube.Solved().Apply(MoveNotation.Parse("F R' U B' L D'"));

        var expected = Lines(
            "       R O G",
            "       B W W",
            "       B B B",
            "G Y Y  O R R  Y B O  Y B W",
            "O O G  O G W  R R W  O B Y",
            "B G O  W W W  O Y R  Y Y W",
            "       G G B",
            "       R Y R",
            "       R G G");

        Assert.Equal(expected, _renderer.Render(cube));
    }

    private static string Lines(params string[] lines) =>
        string.Concat(lines.Select(line => line + Environment.NewLine));
}
