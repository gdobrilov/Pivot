namespace RubiksCube.Domain.Tests;

/// <summary>The acceptance test from the brief: F R' U B' L D' from the solved state.</summary>
public class ChallengeSequenceTests
{
    [Fact]
    public void Challenge_sequence_produces_the_state_shown_in_the_brief()
    {
        var cube = Cube.Solved().Apply(MoveNotation.Parse("F R' U B' L D'"));

        Assert.Equal("ROGBWWBBB", cube[Face.Up].ToString());
        Assert.Equal("GYYOOGBGO", cube[Face.Left].ToString());
        Assert.Equal("ORROGWWWW", cube[Face.Front].ToString());
        Assert.Equal("YBORRWOYR", cube[Face.Right].ToString());
        Assert.Equal("YBWOBYYYW", cube[Face.Back].ToString());
        Assert.Equal("GGBRYRRGG", cube[Face.Down].ToString());
    }
}
