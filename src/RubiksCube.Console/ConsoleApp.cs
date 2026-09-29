using RubiksCube.Application.Rendering;
using RubiksCube.Domain;

namespace RubiksCube.Console;

/// <summary>The whole program, with output injected so it can be tested.</summary>
public static class ConsoleApp
{
    /// <summary>The sequence from the brief.</summary>
    public const string ChallengeSequence = "F R' U B' L D'";

    private const string StepsFlag = "--steps";

    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        var showSteps = args.Contains(StepsFlag, StringComparer.OrdinalIgnoreCase);
        var notation = string.Join(' ', args.Where(arg => !arg.Equals(StepsFlag, StringComparison.OrdinalIgnoreCase)));
        if (string.IsNullOrWhiteSpace(notation))
        {
            notation = ChallengeSequence;
        }

        if (!MoveNotation.TryParse(notation, out var moves, out var parseError))
        {
            error.WriteLine($"Error: {parseError}");
            error.WriteLine("Usage: rubiks [--steps] [moves]   e.g. rubiks \"F R' U B' L D'\"");
            return 1;
        }

        var renderer = new ExplodedViewRenderer();
        var cube = Cube.Solved();

        output.WriteLine("Pivot: we turn things around.");
        output.WriteLine("Legend: W White, O Orange, G Green, R Red, B Blue, Y Yellow");
        output.WriteLine();
        output.WriteLine("Initial state (solved):");
        output.WriteLine(renderer.Render(cube));

        if (showSteps)
        {
            foreach (var move in moves)
            {
                cube = cube.Turn(move);
                output.WriteLine($"After {move} ({Describe(move)}):");
                output.WriteLine(renderer.Render(cube));
            }
        }
        else
        {
            cube = cube.Apply(moves);
        }

        output.WriteLine($"Final state after {MoveNotation.Format(moves)}:");
        output.WriteLine(renderer.Render(cube));
        return 0;
    }

    private static string Describe(Move move) => move.Rotation switch
    {
        Rotation.Clockwise => $"{move.Face} face clockwise 90°",
        Rotation.AntiClockwise => $"{move.Face} face anti-clockwise 90°",
        Rotation.Half => $"{move.Face} face 180°",
        _ => move.ToString(),
    };
}
