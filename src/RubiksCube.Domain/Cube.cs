using RubiksCube.Domain.Geometry;

namespace RubiksCube.Domain;

/// <summary>Immutable: every turn returns a new cube. Solved is white up, green front, red right.</summary>
public sealed class Cube : IEquatable<Cube>
{
    public const int DefaultSize = 3;

    private readonly Colour[] _stickers;

    private Cube(int size, Colour[] stickers)
    {
        Size = size;
        _stickers = stickers;
    }

    public int Size { get; }

    public static IReadOnlyDictionary<Face, Colour> SolvedColours { get; } = new Dictionary<Face, Colour>
    {
        [Face.Up] = Colour.White,
        [Face.Left] = Colour.Orange,
        [Face.Front] = Colour.Green,
        [Face.Right] = Colour.Red,
        [Face.Back] = Colour.Blue,
        [Face.Down] = Colour.Yellow,
    };

    /// <summary>Every face shows a single colour.</summary>
    public bool IsSolved => Faces.All.All(face => this[face].IsUniform);

    public FaceGrid this[Face face] => FaceGrid.FromCells(Size, FaceSlice(face));

    public Colour this[Face face, int row, int column]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Size);
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Size);
            return _stickers[new StickerPosition(face, row, column).Index(Size)];
        }
    }

    public static Cube Solved(int size = DefaultSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 2);
        var stickersPerFace = size * size;
        var stickers = new Colour[StickerCount(size)];
        foreach (var face in Faces.All)
        {
            Array.Fill(stickers, SolvedColours[face], (int)face * stickersPerFace, stickersPerFace);
        }

        return new Cube(size, stickers);
    }

    /// <summary>Inverse of <see cref="ToFacelets"/>; whitespace is ignored.</summary>
    public static Cube FromFacelets(string facelets)
    {
        ArgumentNullException.ThrowIfNull(facelets);
        var symbols = facelets.Where(c => !char.IsWhiteSpace(c)).ToArray();
        var size = (int)Math.Round(Math.Sqrt(symbols.Length / (double)Faces.All.Count));
        if (size < 2 || StickerCount(size) != symbols.Length)
        {
            throw new FormatException($"A facelet string must contain 6·n² symbols; got {symbols.Length}.");
        }

        return new Cube(size, symbols.Select(ColourExtensions.FromSymbol).ToArray());
    }

    public Cube Turn(Move move)
    {
        var permutation = FaceGeometry.PermutationFor(move, Size);
        var next = new Colour[_stickers.Length];
        permutation.Apply<Colour>(_stickers, next);
        return new Cube(Size, next);
    }

    public Cube Apply(IEnumerable<Move> moves)
    {
        ArgumentNullException.ThrowIfNull(moves);
        return moves.Aggregate(this, (cube, move) => cube.Turn(move));
    }

    public Cube Apply(params Move[] moves) => Apply((IEnumerable<Move>)moves);

    /// <summary>All stickers as letters, face by face (U L F R B D), no separators.</summary>
    public string ToFacelets() => string.Concat(_stickers.Select(ColourExtensions.ToSymbol));

    /// <summary>Facelets with a space between faces, e.g. "WWWWWWWWW OOOOOOOOO …".</summary>
    public override string ToString() => string.Join(' ', Faces.All.Select(face => this[face].ToString()));

    public bool Equals(Cube? other) => other is not null && Size == other.Size && _stickers.AsSpan().SequenceEqual(other._stickers);

    public override bool Equals(object? obj) => Equals(obj as Cube);

    public override int GetHashCode()
    {
        var hash = default(HashCode);
        hash.Add(Size);
        foreach (var sticker in _stickers)
        {
            hash.Add(sticker);
        }

        return hash.ToHashCode();
    }

    internal static int StickerCount(int size) => Faces.All.Count * size * size;

    private ArraySegment<Colour> FaceSlice(Face face)
    {
        if (!Enum.IsDefined(face))
        {
            throw new ArgumentOutOfRangeException(nameof(face), face, "Unknown face.");
        }

        var stickersPerFace = Size * Size;
        return new ArraySegment<Colour>(_stickers, (int)face * stickersPerFace, stickersPerFace);
    }
}
