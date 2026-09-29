using RubiksCube.Application.Rendering;
using RubiksCube.Domain;

// The sequence from the brief.
const string ChallengeSequence = "F R' U B' L D'";
const string StepsFlag = "--steps";

var showSteps = args.Contains(StepsFlag, StringComparer.OrdinalIgnoreCase);
var notation = string.Join(' ', args.Where(arg => !arg.Equals(StepsFlag, StringComparison.OrdinalIgnoreCase)));
if (string.IsNullOrWhiteSpace(notation))
{
    notation = ChallengeSequence;
}

if (!MoveNotation.TryParse(notation, out var moves, out var error))
{
    Console.Error.WriteLine($"Error: {error}");
    Console.Error.WriteLine("Usage: rubiks [--steps] [moves]   e.g. rubiks \"F R' U B' L D'\"");
    return 1;
}

var renderer = new ExplodedViewRenderer();
var cube = Cube.Solved();

Console.WriteLine("Rubik's cube simulator (TTC challenge)");
Console.WriteLine("Legend: W White, O Orange, G Green, R Red, B Blue, Y Yellow");
Console.WriteLine();
Console.WriteLine("Initial state (solved):");
Console.WriteLine(renderer.Render(cube));

if (showSteps)
{
    foreach (var move in moves)
    {
        cube = cube.Turn(move);
        Console.WriteLine($"After {move} ({Describe(move)}):");
        Console.WriteLine(renderer.Render(cube));
    }
}
else
{
    cube = cube.Apply(moves);
}

Console.WriteLine($"Final state after {MoveNotation.Format(moves)}:");
Console.WriteLine(renderer.Render(cube));
return 0;

static string Describe(Move move) => move.Rotation switch
{
    Rotation.Clockwise => $"{move.Face} face clockwise 90°",
    Rotation.AntiClockwise => $"{move.Face} face anti-clockwise 90°",
    Rotation.Half => $"{move.Face} face 180°",
    _ => move.ToString(),
};
