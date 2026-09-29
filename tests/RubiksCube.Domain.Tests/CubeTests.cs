namespace RubiksCube.Domain.Tests;

public class CubeTests
{
    [Fact]
    public void Solved_cube_has_white_up_green_front_and_red_right()
    {
        var cube = Cube.Solved();

        Assert.True(cube.IsSolved);
        Assert.Equal(3, cube.Size);
        Assert.Equal("WWWWWWWWW OOOOOOOOO GGGGGGGGG RRRRRRRRR BBBBBBBBB YYYYYYYYY", cube.ToString());
    }

    [Fact]
    public void Turn_returns_a_new_cube_and_leaves_the_original_unchanged()
    {
        var original = Cube.Solved();

        var turned = original.Turn(Move.Clockwise(Face.Front));

        Assert.True(original.IsSolved);
        Assert.False(turned.IsSolved);
    }

    [Fact]
    public void Turning_a_face_rotates_its_own_stickers_clockwise()
    {
        var before = Cube.Solved().Apply(MoveNotation.Parse("R U' D"));

        var after = before.Turn(Move.Clockwise(Face.Front));

        // The left column, read bottom-up, becomes the top row; the top row becomes the right column.
        Assert.Equal(before[Face.Front].Column(0).Reverse(), after[Face.Front].Row(0));
        Assert.Equal(before[Face.Front].Row(0), after[Face.Front].Column(2));
        Assert.Equal(before[Face.Front, 1, 1], after[Face.Front, 1, 1]);
    }

    [Theory]
    [MemberData(nameof(TestData.AllFaces), MemberType = typeof(TestData))]
    public void Four_quarter_turns_return_to_the_start(Face face)
    {
        var move = Move.Clockwise(face);

        Assert.Equal(Cube.Solved(), Cube.Solved().Apply(move, move, move, move));
    }

    [Theory]
    [MemberData(nameof(TestData.AllFaces), MemberType = typeof(TestData))]
    public void Centre_stickers_never_move_on_a_3x3(Face face)
    {
        var scrambled = Cube.Solved().Apply(MoveNotation.Parse("F R' U B' L D' F2 R U2"));

        Assert.Equal(Cube.SolvedColours[face], scrambled[face, 1, 1]);
    }

    [Fact]
    public void R_U_R_prime_U_prime_six_times_is_the_identity()
    {
        var moves = MoveNotation.Parse("R U R' U'");

        var cube = Enumerable.Range(0, 6).Aggregate(Cube.Solved(), (current, _) => current.Apply(moves));

        Assert.True(cube.IsSolved);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void A_scramble_keeps_the_same_number_of_stickers_of_each_colour(int size)
    {
        var cube = Cube.Solved(size).Apply(MoveNotation.Parse("F R' U B' L D'"));

        var counts = Faces.All.SelectMany(face => cube[face].Cells).GroupBy(colour => colour).ToDictionary(g => g.Key, g => g.Count());

        Assert.All(Enum.GetValues<Colour>(), colour => Assert.Equal(size * size, counts[colour]));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void The_model_is_not_tied_to_three_by_three(int size)
    {
        var move = Move.Clockwise(Face.Right);

        var cube = Cube.Solved(size).Apply(move, move, move, move);

        Assert.True(cube.IsSolved);
        Assert.False(Cube.Solved(size).Turn(move).IsSolved);
    }

    [Fact]
    public void Facelets_round_trip()
    {
        var cube = Cube.Solved().Apply(MoveNotation.Parse("F R' U"));

        Assert.Equal(cube, Cube.FromFacelets(cube.ToFacelets()));
        Assert.Equal(cube, Cube.FromFacelets(cube.ToString()));
    }

    [Theory]
    [InlineData("WWW")]
    [InlineData("WOGRBY")]
    public void Facelets_of_the_wrong_length_are_rejected(string facelets)
    {
        Assert.Throws<FormatException>(() => Cube.FromFacelets(facelets));
    }

    [Fact]
    public void Facelets_with_an_unknown_colour_are_rejected()
    {
        var facelets = Cube.Solved().ToFacelets().Replace('W', 'X');

        Assert.Throws<FormatException>(() => Cube.FromFacelets(facelets));
    }

    [Fact]
    public void Cubes_with_the_same_stickers_are_equal()
    {
        var first = Cube.Solved().Apply(MoveNotation.Parse("F R"));
        var second = Cube.Solved().Apply(MoveNotation.Parse("F R"));

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.NotEqual(first, Cube.Solved());
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 3)]
    public void Indexer_rejects_positions_outside_the_face(int row, int column)
    {
        var cube = Cube.Solved();

        Assert.Throws<ArgumentOutOfRangeException>(() => cube[Face.Front, row, column]);
    }

    [Fact]
    public void Unknown_faces_and_rotations_are_rejected()
    {
        var cube = Cube.Solved();

        Assert.Throws<ArgumentOutOfRangeException>(() => cube[(Face)9]);
        Assert.Throws<ArgumentOutOfRangeException>(() => cube[(Face)9, 0, 0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => cube.Turn(new Move((Face)9, Rotation.Clockwise)));
        Assert.Throws<ArgumentOutOfRangeException>(() => cube.Turn(new Move(Face.Up, (Rotation)9)));
    }
}
