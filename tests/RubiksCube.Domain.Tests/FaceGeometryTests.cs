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
    [InlineData("R'", "WWBWWBWWB OOOOOOOOO GGWGGWGGW RRRRRRRRR YBBYBBYBB YYGYYGYYG")]
    [InlineData("F2", "WWWWWWYYY OOROOROOR GGGGGGGGG ORRORRORR BBBBBBBBB WWWYYYYYY")]
    public void Single_turn_moves_the_expected_stickers(string move, string expectedFacelets)
    {
        var cube = Cube.Solved().Apply(MoveNotation.Parse(move));

        Assert.Equal(expectedFacelets.Replace(" ", string.Empty, StringComparison.Ordinal), cube.ToFacelets());
    }

    [Fact]
    public void Front_turn_drags_the_four_neighbouring_strips_round_clockwise()
    {
        var cube = Cube.Solved().Turn(Move.Clockwise(Face.Front));

        // What was on Left is now on the bottom row of Up.
        Assert.Equal([Colour.Orange, Colour.Orange, Colour.Orange], cube[Face.Up].Row(2));
        // What was on Up is now on the left column of Right.
        Assert.Equal([Colour.White, Colour.White, Colour.White], cube[Face.Right].Column(0));
        // What was on Right is now on the top row of Down.
        Assert.Equal([Colour.Red, Colour.Red, Colour.Red], cube[Face.Down].Row(0));
        // What was on Down is now on the right column of Left.
        Assert.Equal([Colour.Yellow, Colour.Yellow, Colour.Yellow], cube[Face.Left].Column(2));
        // Back is untouched.
        Assert.True(cube[Face.Back].IsUniform);
    }

    [Fact]
    public void Reading_direction_follows_the_corner_met_first()
    {
        // Scramble so that every sticker is distinguishable by position, then turn Front.
        var before = Cube.Solved().Apply(MoveNotation.Parse("U R D' L B"));
        var after = before.Turn(Move.Clockwise(Face.Front));

        // Up's bottom row, read from its bottom-left corner, lands on Right's left column read from the top.
        Assert.Equal(before[Face.Up].Row(2), after[Face.Right].Column(0));
        // Right's left column, read from the top, lands on Down's top row read from the right.
        Assert.Equal(before[Face.Right].Column(0), after[Face.Down].Row(0).Reverse());
        // Down's top row, read from the right, lands on Left's right column read from the bottom.
        Assert.Equal(before[Face.Down].Row(0).Reverse(), after[Face.Left].Column(2).Reverse());
        // Left's right column, read from the bottom, lands on Up's bottom row read from the left.
        Assert.Equal(before[Face.Left].Column(2).Reverse(), after[Face.Up].Row(2));
    }

    [Theory]
    [MemberData(nameof(AllFaces))]
    public void Every_face_has_four_neighbouring_strips_that_never_touch_the_face_itself_or_its_opposite(Face face)
    {
        var strips = FaceGeometry.NeighboursOf(face);

        Assert.Equal(4, strips.Count);
        Assert.DoesNotContain(strips, strip => strip.Face == face);
        Assert.Equal(4, strips.Select(strip => strip.Face).Distinct().Count());
    }

    [Fact]
    public void Each_strip_reads_size_stickers_along_its_side()
    {
        var strip = new EdgeStrip(Face.Down, Side.Top, Corner.TopRight);

        var positions = strip.Positions(3);

        Assert.Equal([new(Face.Down, 0, 2), new(Face.Down, 0, 1), new(Face.Down, 0, 0)], positions);
    }

    [Fact]
    public void A_corner_that_is_not_on_the_side_is_rejected()
    {
        var strip = new EdgeStrip(Face.Down, Side.Top, Corner.BottomLeft);

        Assert.Throws<InvalidOperationException>(() => strip.Positions(3));
    }

    public static TheoryData<Face> AllFaces => new(Faces.All);
}
