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
    public void Rows_and_columns_are_read_as_seen_from_outside()
    {
        Assert.Equal([Colour.White, Colour.Orange, Colour.Green], Grid.Row(0));
        Assert.Equal([Colour.White, Colour.Red, Colour.White], Grid.Column(0));
        Assert.Equal(Colour.Yellow, Grid[1, 2]);
        Assert.Equal("WOGRBYWRB", Grid.ToString());
    }

    [Fact]
    public void Uniform_means_a_single_colour()
    {
        Assert.True(FaceGrid.FromCells(2, [Colour.Green, Colour.Green, Colour.Green, Colour.Green]).IsUniform);
        Assert.False(Grid.IsUniform);
    }

    [Fact]
    public void Grids_with_the_same_stickers_are_equal()
    {
        var copy = FaceGrid.FromCells(3, Grid.Cells);

        Assert.Equal(Grid, copy);
        Assert.Equal(Grid.GetHashCode(), copy.GetHashCode());
    }

    [Fact]
    public void Wrong_number_of_cells_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => FaceGrid.FromCells(3, [Colour.White]));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Positions_outside_the_grid_are_rejected(int index)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Grid.Row(index));
        Assert.Throws<ArgumentOutOfRangeException>(() => Grid.Column(index));
        Assert.Throws<ArgumentOutOfRangeException>(() => Grid[index, 0]);
    }
}
