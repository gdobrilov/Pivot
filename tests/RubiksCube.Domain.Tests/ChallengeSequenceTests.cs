
namespace RubiksCube.Domain.Tests;

/// <summary>The acceptance test from the TTC brief: F R' U B' L D' from the solved state.</summary>
public class ChallengeSequenceTests
{
    public const string Sequence = "F R' U B' L D'";

    [Fact]
    public void Challenge_sequence_produces_the_state_shown_in_the_brief()
    {
        var cube = Cube.Solved().Apply(MoveNotation.Parse(Sequence));

        Assert.Equal("ROGBWWBBB", cube[Face.Up].ToString());
        Assert.Equal("GYYOOGBGO", cube[Face.Left].ToString());
        Assert.Equal("ORROGWWWW", cube[Face.Front].ToString());
        Assert.Equal("YBORRWOYR", cube[Face.Right].ToString());
        Assert.Equal("YBWOBYYYW", cube[Face.Back].ToString());
        Assert.Equal("GGBRYRRGG", cube[Face.Down].ToString());
    }

    [Fact]
    public void Reversing_the_challenge_sequence_solves_the_cube_again()
    {
        var moves = MoveNotation.Parse(Sequence);
        var scrambled = Cube.Solved().Apply(moves);

        var restored = scrambled.Apply(moves.Reverse().Select(move => move.Inverse));

        Assert.True(restored.IsSolved);
    }
}
