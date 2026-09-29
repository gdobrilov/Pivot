namespace RubiksCube.Domain;

/// <summary>The stickers of one face, N×N, as seen from outside: row 0 is the top, column 0 is the left.</summary>
public sealed class FaceGrid : IEquatable<FaceGrid>
{
    private readonly Colour[] _cells;

    private FaceGrid(int size, Colour[] cells)
    {
        Size = size;
        _cells = cells;
    }

    public int Size { get; }

    /// <summary>All stickers, row by row.</summary>
    public IReadOnlyList<Colour> Cells => Array.AsReadOnly(_cells);

    public bool IsUniform => Array.TrueForAll(_cells, colour => colour == _cells[0]);

    public Colour this[int row, int column]
    {
        get
        {
            ValidateIndex(row, nameof(row));
            ValidateIndex(column, nameof(column));
            return _cells[(row * Size) + column];
        }
    }

    public static FaceGrid FromCells(int size, IEnumerable<Colour> cells)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        ArgumentNullException.ThrowIfNull(cells);
        var array = cells.ToArray();
        if (array.Length != size * size)
        {
            throw new ArgumentException($"Expected {size * size} stickers but got {array.Length}.", nameof(cells));
        }

        return new FaceGrid(size, array);
    }

    public IReadOnlyList<Colour> Row(int row)
    {
        ValidateIndex(row, nameof(row));
        return Array.AsReadOnly(_cells[(row * Size)..((row + 1) * Size)]);
    }

    public IReadOnlyList<Colour> Column(int column)
    {
        ValidateIndex(column, nameof(column));
        return Enumerable.Range(0, Size).Select(row => _cells[(row * Size) + column]).ToArray();
    }

    /// <summary>(row, column) moves to (column, Size − 1 − row): the left column, read bottom-up, becomes the top row.</summary>
    public FaceGrid RotatedClockwise()
    {
        var rotated = new Colour[_cells.Length];
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                rotated[(column * Size) + (Size - 1 - row)] = _cells[(row * Size) + column];
            }
        }

        return new FaceGrid(Size, rotated);
    }

    public bool Equals(FaceGrid? other) => other is not null && Size == other.Size && _cells.AsSpan().SequenceEqual(other._cells);

    public override bool Equals(object? obj) => Equals(obj as FaceGrid);

    public override int GetHashCode()
    {
        var hash = default(HashCode);
        hash.Add(Size);
        foreach (var cell in _cells)
        {
            hash.Add(cell);
        }

        return hash.ToHashCode();
    }

    /// <summary>One letter per sticker, row by row.</summary>
    public override string ToString() => string.Concat(_cells.Select(ColourExtensions.ToSymbol));

    private void ValidateIndex(int value, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, name);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, Size, name);
    }
}
