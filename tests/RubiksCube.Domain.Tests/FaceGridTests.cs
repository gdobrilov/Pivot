using RubiksCube.Domain;

namespace RubiksCube.Domain.Tests;

public class FaceGridTests
{
    private static readonly FaceGrid Grid = FaceGrid.FromCells(3,
    [
        Colour.White, Colour.Orange, Colour.Green,
        Colour.Red, Colour.Blue, Colour.Yellow,
        Colour.White, Colour.Red, Colour.Blue,
    ]);

    [Fact]
    public void Rows_and_columns_are_read_in_net_orientation()
    {
        Assert.Equal([Colour.White, Colour.Orange, Colour.Green], Grid.Row(0));
        Assert.Equal([Colour.White, Colour.Red, Colour.White], Grid.Column(0));
        Assert.Equal(Colour.Yellow, Grid[1, 2]);
    }

    [Fact]
    public void Rotated_clockwise_turns_the_left_column_into_the_top_row()
    {
        var rotated = Grid.RotatedClockwise();

        // Left column read bottom-up becomes the top row.
        Assert.Equal([Colour.White, Colour.Red, Colour.White], rotated.Row(0));
        Assert.Equal([Colour.Red, Colour.Blue, Colour.Orange], rotated.Row(1));
        Assert.Equal([Colour.Blue, Colour.Yellow, Colour.Green], rotated.Row(2));
        Assert.Equal(Colour.Blue, rotated[1, 1]);
    }

    [Fact]
    public void Four_clockwise_rotations_are_the_identity()
    {
        var rotated = Grid.RotatedClockwise().RotatedClockwise().RotatedClockwise().RotatedClockwise();

        Assert.Equal(Grid, rotated);
    }

    [Fact]
    public void Uniform_means_a_single_colour()
    {
        Assert.True(FaceGrid.Filled(3, Colour.Green).IsUniform);
        Assert.False(Grid.IsUniform);
    }

    [Fact]
    public void Wrong_number_of_cells_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => FaceGrid.FromCells(3, [Colour.White]));
    }
}
