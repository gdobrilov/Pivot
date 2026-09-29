using System.Collections.Frozen;
using RubiksCube.Domain.Geometry;

namespace RubiksCube.Domain;

/// <summary>Immutable: every turn returns a new cube. Solved is white up, green front, red right.</summary>
public sealed class Cube : IEquatable<Cube>
{
    public const int DefaultSize = 3;

    private const int MinimumSize = 2;

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
    }.ToFrozenDictionary();

    /// <summary>Every face shows a single colour.</summary>
    public bool IsSolved => Faces.All.All(face => this[face].IsUniform);

    public FaceGrid this[Face face]
    {
        get
        {
            ValidateFace(face);
            var stickersPerFace = Size * Size;
            return FaceGrid.FromCells(Size, new ArraySegment<Colour>(_stickers, (int)face * stickersPerFace, stickersPerFace));
        }
    }

    public Colour this[Face face, int row, int column]
    {
        get
        {
            ValidateFace(face);
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Size);
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Size);
            return _stickers[new StickerPosition(face, row, column).Index(Size)];
        }
    }

    public static Cube Solved(int size = DefaultSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, MinimumSize);
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
        if (size < MinimumSize || StickerCount(size) != symbols.Length)
        {
            throw new FormatException($"Expected 6 faces of n*n symbols with n >= {MinimumSize}, got {symbols.Length} symbols.");
        }

        return new Cube(size, symbols.Select(ColourExtensions.FromSymbol).ToArray());
    }

    public Cube Turn(Move move)
    {
        ValidateFace(move.Face);
        if (!Enum.IsDefined(move.Rotation))
        {
            throw new ArgumentOutOfRangeException(nameof(move), move.Rotation, "Unknown rotation.");
        }

        return new Cube(Size, FaceGeometry.PermutationFor(move, Size).Apply(_stickers));
    }

    public Cube Apply(params IEnumerable<Move> moves)
    {
        ArgumentNullException.ThrowIfNull(moves);
        return moves.Aggregate(this, (cube, move) => cube.Turn(move));
    }

    /// <summary>All stickers as letters, face by face (U L F R B D), no separators.</summary>
    public string ToFacelets() => string.Concat(_stickers.Select(ColourExtensions.ToSymbol));

    /// <summary>Facelets with a space between faces, e.g. "WWWWWWWWW OOOOOOOOO ...".</summary>
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

    private static void ValidateFace(Face face)
    {
        if (!Enum.IsDefined(face))
        {
            throw new ArgumentOutOfRangeException(nameof(face), face, "Unknown face.");
        }
    }
}
