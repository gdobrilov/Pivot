namespace RubiksCube.Domain.Tests;

public class MoveNotationTests
{
    [Fact]
    public void Parses_the_challenge_sequence()
    {
        Assert.Equal(
            [
                Move.Clockwise(Face.Front),
                Move.AntiClockwise(Face.Right),
                Move.Clockwise(Face.Up),
                Move.AntiClockwise(Face.Back),
                Move.Clockwise(Face.Left),
                Move.AntiClockwise(Face.Down),
            ],
            MoveNotation.Parse("F R' U B' L D'"));
    }

    [Fact]
    public void Half_turn_is_a_single_move()
    {
        Assert.Equal([Move.Half(Face.Front)], MoveNotation.Parse("F2"));
        Assert.Equal("F2", Move.Half(Face.Front).ToString());
    }

    [Theory]
    [InlineData("F,R',U2")]
    [InlineData("  F   R'\tU2 ")]
    [InlineData("F\nR'\r\nU2")]
    public void Accepts_spaces_tabs_new_lines_and_commas_as_separators(string notation)
    {
        Assert.Equal("F R' U2", MoveNotation.Format(MoveNotation.Parse(notation)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Empty_input_parses_to_no_moves(string? notation)
    {
        var ok = MoveNotation.TryParse(notation, out var moves, out var error);

        Assert.True(ok);
        Assert.Empty(moves);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("f")]
    [InlineData("F3")]
    [InlineData("F''")]
    [InlineData("FR")]
    public void Rejects_invalid_tokens_with_a_helpful_message(string notation)
    {
        var ok = MoveNotation.TryParse(notation, out var moves, out var error);

        Assert.False(ok);
        Assert.Empty(moves);
        Assert.Contains($"'{notation}' is not a valid move", error, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_throws_for_invalid_notation()
    {
        Assert.Throws<FormatException>(() => MoveNotation.Parse("F Q"));
    }

    [Fact]
    public void Format_round_trips_through_parse()
    {
        const string notation = "U' L F' R2 B' D";

        Assert.Equal(notation, MoveNotation.Format(MoveNotation.Parse(notation)));
    }
}
