
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
        Assert.Equal(Colour.White, cube[Face.Up, 1, 1]);
        Assert.Equal(Colour.Green, cube[Face.Front, 1, 1]);
        Assert.Equal(Colour.Red, cube[Face.Right, 1, 1]);
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
    public void Turning_a_face_rotates_its_own_grid_clockwise()
    {
        var scrambled = Cube.Solved().Apply(MoveNotation.Parse("R U' D"));

        var turned = scrambled.Turn(Move.Clockwise(Face.Front));

        Assert.Equal(scrambled[Face.Front].RotatedClockwise(), turned[Face.Front]);
    }

    [Theory]
    [MemberData(nameof(AllFaces))]
    public void Four_quarter_turns_return_to_the_start(Face face)
    {
        var move = Move.Clockwise(face);

        Assert.Equal(Cube.Solved(), Cube.Solved().Apply(move, move, move, move));
    }

    [Theory]
    [MemberData(nameof(AllFaces))]
    public void A_move_followed_by_its_inverse_is_the_identity(Face face)
    {
        foreach (var rotation in Enum.GetValues<Rotation>())
        {
            var move = new Move(face, rotation);
            Assert.True(Cube.Solved().Apply(move, move.Inverse).IsSolved, $"{move} then {move.Inverse}");
        }
    }

    [Theory]
    [MemberData(nameof(AllFaces))]
    public void Anti_clockwise_equals_three_clockwise_and_half_equals_two(Face face)
    {
        var clockwise = Move.Clockwise(face);

        Assert.Equal(Cube.Solved().Apply(clockwise, clockwise, clockwise), Cube.Solved().Turn(Move.AntiClockwise(face)));
        Assert.Equal(Cube.Solved().Apply(clockwise, clockwise), Cube.Solved().Turn(Move.Half(face)));
    }

    [Theory]
    [MemberData(nameof(AllFaces))]
    public void Centre_stickers_never_move_on_a_3x3(Face face)
    {
        var scrambled = Cube.Solved().Apply(MoveNotation.Parse("F R' U B' L D' F2 R U2"));

        Assert.Equal(Cube.SolvedColours[face], scrambled[face, 1, 1]);
    }

    [Fact]
    public void Sexy_move_repeated_six_times_is_the_identity()
    {
        var sexyMove = MoveNotation.Parse("R U R' U'");

        var cube = Enumerable.Range(0, 6).Aggregate(Cube.Solved(), (current, _) => current.Apply(sexyMove));

        Assert.True(cube.IsSolved);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Every_turn_preserves_the_number_of_stickers_of_each_colour(int size)
    {
        var cube = Cube.Solved(size).Apply(MoveNotation.Parse("F R' U B' L D'"));

        var counts = Faces.All.SelectMany(face => cube[face].Cells).GroupBy(colour => colour).ToDictionary(g => g.Key, g => g.Count());

        Assert.All(Enum.GetValues<Colour>(), colour => Assert.Equal(size * size, counts[colour]));
    }

    [Fact]
    public void The_model_is_not_tied_to_three_by_three()
    {
        var move = Move.Clockwise(Face.Right);

        var twoByTwo = Cube.Solved(2).Apply(move, move.Inverse);
        var fourByFour = Cube.Solved(4).Apply(move, move, move, move);

        Assert.True(twoByTwo.IsSolved);
        Assert.True(fourByFour.IsSolved);
    }

    [Fact]
    public void Facelets_round_trip()
    {
        var cube = Cube.Solved().Apply(MoveNotation.Parse("F R' U"));

        Assert.Equal(cube, Cube.FromFacelets(cube.ToFacelets()));
        Assert.Equal(cube, Cube.FromFacelets(cube.ToString()));
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
    [InlineData(0, 3)]
    public void Indexer_rejects_positions_outside_the_face(int row, int column)
    {
        var cube = Cube.Solved();

        Assert.Throws<ArgumentOutOfRangeException>(() => cube[Face.Front, row, column]);
    }

    public static TheoryData<Face> AllFaces => new(Faces.All);
}
