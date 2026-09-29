using RubiksCube.Domain.Geometry;

namespace RubiksCube.Domain.Tests;

/// <summary>Expected facelets (U L F R B D) were checked against rubiks-cube-solver.com.</summary>
public class FaceGeometryTests
{
    [Theory]
    [InlineData("F", "WWWWWWOOO OOYOOYOOY GGGGGGGGG WRRWRRWRR BBBBBBBBB RRRYYYYYY")]
    [InlineData("R", "WWGWWGWWG OOOOOOOOO GGYGGYGGY RRRRRRRRR WBBWBBWBB YYBYYBYYB")]
    [InlineData("U", "WWWWWWWWW GGGOOOOOO RRRGGGGGG BBBRRRRRR OOOBBBBBB YYYYYYYYY")]
    [InlineData("B", "RRRWWWWWW WOOWOOWOO GGGGGGGGG RRYRRYRRY BBBBBBBBB YYYYYYOOO")]
    [InlineData("L", "BWWBWWBWW OOOOOOOOO WGGWGGWGG RRRRRRRRR BBYBBYBBY GYYGYYGYY")]
    [InlineData("D", "WWWWWWWWW OOOOOOBBB GGGGGGOOO RRRRRRGGG BBBBBBRRR YYYYYYYYY")]
    [InlineData("F'", "WWWWWWRRR OOWOOWOOW GGGGGGGGG YRRYRRYRR BBBBBBBBB OOOYYYYYY")]
    [InlineData("F2", "WWWWWWYYY OOROOROOR GGGGGGGGG ORRORRORR BBBBBBBBB WWWYYYYYY")]
    public void A_single_turn_from_solved_moves_the_expected_stickers(string moves, string expected)
    {
        Assert.Equal(Facelets(expected), Cube.Solved().Apply(MoveNotation.Parse(moves)).ToFacelets());
    }

    // From solved every strip is one colour, so the cases above cannot catch a strip read the wrong
    // way round. These turn each face once more on a cube that is already mixed.
    [Theory]
    [InlineData("F R", "WWGWWGOOG OOYOOYOOY GGRGGYGGY WWWRRRRRR OBBWBBWBB RRBYYBYYB")]
    [InlineData("R U", "WWWWWWGGG GGYOOOOOO RRRGGYGGY WBBRRRRRR OOOWBBWBB YYBYYBYYB")]
    [InlineData("U F'", "WWWWWWBRR GGWOOWOOW RGGRGGRGG YBBYRRYRR OOOBBBBBB GOOYYYYYY")]
    [InlineData("B L", "BRRBWWBWW WWWOOOOOO RGGWGGWGG RRYRRYRRY BBOBBYBBY GYYGYYGOO")]
    [InlineData("L D2", "BWWBWWBWW OOOOOORRR WGGWGGBBY RRRRRROOO BBYBBYWGG YYGYYGYYG")]
    [InlineData("D B'", "BOOWWWWWW YOOYOOYBB GGGGGGOOO RRWRRWGGW BBRBBRBBR YYYYYYGRR")]
    public void A_turn_on_a_mixed_cube_moves_the_expected_stickers(string moves, string expected)
    {
        Assert.Equal(Facelets(expected), Cube.Solved().Apply(MoveNotation.Parse(moves)).ToFacelets());
    }

    [Fact]
    public void Front_turn_drags_the_four_neighbouring_strips_round_clockwise()
    {
        var cube = Cube.Solved().Turn(Move.Clockwise(Face.Front));

        Assert.Equal([Colour.Orange, Colour.Orange, Colour.Orange], cube[Face.Up].Row(2));
        Assert.Equal([Colour.White, Colour.White, Colour.White], cube[Face.Right].Column(0));
        Assert.Equal([Colour.Red, Colour.Red, Colour.Red], cube[Face.Down].Row(0));
        Assert.Equal([Colour.Yellow, Colour.Yellow, Colour.Yellow], cube[Face.Left].Column(2));
        Assert.True(cube[Face.Back].IsUniform);
    }

    [Fact]
    public void Each_strip_is_read_from_the_corner_met_first()
    {
        // After this scramble none of the four strips around Front reads the same both ways,
        // so reading any of them backwards would fail one of the assertions.
        var before = Cube.Solved().Apply(MoveNotation.Parse("R U B' D L'"));
        var after = before.Turn(Move.Clockwise(Face.Front));

        Assert.Equal(before[Face.Up].Row(2), after[Face.Right].Column(0));
        Assert.Equal(before[Face.Right].Column(0), after[Face.Down].Row(0).Reverse());
        Assert.Equal(before[Face.Down].Row(0).Reverse(), after[Face.Left].Column(2).Reverse());
        Assert.Equal(before[Face.Left].Column(2).Reverse(), after[Face.Up].Row(2));
    }

    [Fact]
    public void A_strip_read_from_the_far_corner_is_reversed()
    {
        var strip = new EdgeStrip(Face.Down, Side.Top, Corner.TopRight);

        Assert.Equal([new(Face.Down, 0, 2), new(Face.Down, 0, 1), new(Face.Down, 0, 0)], strip.Positions(3));
    }

    [Fact]
    public void A_corner_that_is_not_on_the_side_is_rejected()
    {
        var strip = new EdgeStrip(Face.Down, Side.Top, Corner.BottomLeft);

        Assert.Throws<InvalidOperationException>(() => strip.Positions(3));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Every_move_moves_each_sticker_to_exactly_one_place(int size)
    {
        var moves = Faces.All.SelectMany(face => Enum.GetValues<Rotation>().Select(rotation => new Move(face, rotation)));

        Assert.All(moves, move => Assert.True(FaceGeometry.PermutationFor(move, size).IsBijection(), move.ToString()));
    }

    private static string Facelets(string grouped) => grouped.Replace(" ", string.Empty, StringComparison.Ordinal);
}
