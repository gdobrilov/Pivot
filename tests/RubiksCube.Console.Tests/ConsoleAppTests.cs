namespace RubiksCube.Console.Tests;

public class ConsoleAppTests
{
    [Fact]
    public void Without_arguments_prints_the_challenge_result_from_the_brief()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = ConsoleApp.Run([], output, error);

        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());
        var text = output.ToString();
        Assert.Contains("Final state after F R' U B' L D':", text, StringComparison.Ordinal);
        Assert.Contains(
            Lines(
                "       R O G",
                "       B W W",
                "       B B B",
                "G Y Y  O R R  Y B O  Y B W",
                "O O G  O G W  R R W  O B Y",
                "B G O  W W W  O Y R  Y Y W",
                "       G G B",
                "       R Y R",
                "       R G G"),
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Steps_flag_prints_the_cube_after_every_move_with_a_description()
    {
        var output = new StringWriter();

        var exitCode = ConsoleApp.Run(["--steps", "R'", "U2"], output, TextWriter.Null);

        Assert.Equal(0, exitCode);
        Assert.Contains("After R' (Right face anti-clockwise 90°):", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("After U2 (Up face 180°):", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Invalid_notation_explains_the_problem_and_exits_with_1()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = ConsoleApp.Run(["F", "X"], output, error);

        Assert.Equal(1, exitCode);
        Assert.Empty(output.ToString());
        Assert.Contains("'X' is not a valid move", error.ToString(), StringComparison.Ordinal);
        Assert.Contains("Usage:", error.ToString(), StringComparison.Ordinal);
    }

    private static string Lines(params string[] lines) =>
        string.Concat(lines.Select(line => line + Environment.NewLine));
}
