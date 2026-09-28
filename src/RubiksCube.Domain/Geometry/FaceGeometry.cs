using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace RubiksCube.Domain.Geometry;

/// <summary>
/// The only place that knows how the six faces are glued together. It describes what a turn of
/// each face does to the stickers as a <see cref="Permutation"/>, which <see cref="Cube.Turn"/>
/// then simply applies.
/// </summary>
/// <remarks>
/// <para>A clockwise turn of a face does two independent things:</para>
/// <list type="number">
///   <item>the face's own grid rotates clockwise (<see cref="FaceGrid.RotatedClockwise"/>), and</item>
///   <item>the four strips of stickers on the neighbouring faces move round one place.</item>
/// </list>
/// <para>
/// <see cref="Neighbours"/> lists, for each face, those four strips in the order they are met when
/// travelling clockwise around the turning face, each read from the corner met first. Sticker k
/// of one strip lands on sticker k of the next, and the last strip wraps to the first. Faces are
/// drawn as in the net (U above F; L, F, R, B in a row; D below F), so Back is seen from behind.
/// </para>
/// <para>An anti-clockwise turn is the inverse permutation; a half turn is the clockwise one applied twice.</para>
/// </remarks>
internal static class FaceGeometry
{
    private static readonly FrozenDictionary<Face, EdgeStrip[]> Neighbours = new Dictionary<Face, EdgeStrip[]>
    {
        [Face.Up] =
        [
            new(Face.Front, Side.Top, Corner.TopLeft),
            new(Face.Left, Side.Top, Corner.TopLeft),
            new(Face.Back, Side.Top, Corner.TopLeft),
            new(Face.Right, Side.Top, Corner.TopLeft),
        ],
        [Face.Down] =
        [
            new(Face.Front, Side.Bottom, Corner.BottomLeft),
            new(Face.Right, Side.Bottom, Corner.BottomLeft),
            new(Face.Back, Side.Bottom, Corner.BottomLeft),
            new(Face.Left, Side.Bottom, Corner.BottomLeft),
        ],
        [Face.Front] =
        [
            new(Face.Up, Side.Bottom, Corner.BottomLeft),
            new(Face.Right, Side.Left, Corner.TopLeft),
            new(Face.Down, Side.Top, Corner.TopRight),
            new(Face.Left, Side.Right, Corner.BottomRight),
        ],
        [Face.Back] =
        [
            new(Face.Up, Side.Top, Corner.TopLeft),
            new(Face.Left, Side.Left, Corner.BottomLeft),
            new(Face.Down, Side.Bottom, Corner.BottomRight),
            new(Face.Right, Side.Right, Corner.TopRight),
        ],
        [Face.Left] =
        [
            new(Face.Up, Side.Left, Corner.TopLeft),
            new(Face.Front, Side.Left, Corner.TopLeft),
            new(Face.Down, Side.Left, Corner.TopLeft),
            new(Face.Back, Side.Right, Corner.BottomRight),
        ],
        [Face.Right] =
        [
            new(Face.Front, Side.Right, Corner.TopRight),
            new(Face.Up, Side.Right, Corner.TopRight),
            new(Face.Back, Side.Left, Corner.BottomLeft),
            new(Face.Down, Side.Right, Corner.TopRight),
        ],
    }.ToFrozenDictionary();

    private static readonly ConcurrentDictionary<(int Size, Move Move), Permutation> Cache = new();

    /// <summary>The permutation for <paramref name="move"/> on a cube of the given size, computed once and cached.</summary>
    public static Permutation PermutationFor(Move move, int size) =>
        Cache.GetOrAdd((size, move), key => Build(key.Move, key.Size));

    /// <summary>The strips that move when <paramref name="face"/> turns, in clockwise order. Exposed for tests.</summary>
    internal static IReadOnlyList<EdgeStrip> NeighboursOf(Face face) => Neighbours[face];

    private static Permutation Build(Move move, int size)
    {
        var clockwise = ClockwiseTurn(move.Face, size);
        return move.Rotation switch
        {
            Rotation.Clockwise => clockwise,
            Rotation.AntiClockwise => clockwise.Inverse(),
            Rotation.Half => clockwise.Then(clockwise),
            _ => throw new ArgumentOutOfRangeException(nameof(move), move.Rotation, "Unknown rotation."),
        };
    }

    private static Permutation ClockwiseTurn(Face face, int size) =>
        Permutation.FromMappings(Cube.StickerCount(size), OwnGridMappings(face, size).Concat(NeighbourMappings(face, size)));

    /// <summary>The turning face's own stickers: (row, column) moves to (column, size − 1 − row).</summary>
    private static IEnumerable<(int Target, int Source)> OwnGridMappings(Face face, int size)
    {
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                var source = new StickerPosition(face, row, column);
                var target = new StickerPosition(face, column, size - 1 - row);
                yield return (target.Index(size), source.Index(size));
            }
        }
    }

    /// <summary>Each neighbouring strip moves onto the next one, sticker k onto sticker k.</summary>
    private static IEnumerable<(int Target, int Source)> NeighbourMappings(Face face, int size)
    {
        var strips = Neighbours[face];
        for (var i = 0; i < strips.Length; i++)
        {
            var source = strips[i].Positions(size);
            var target = strips[(i + 1) % strips.Length].Positions(size);
            for (var k = 0; k < size; k++)
            {
                yield return (target[k].Index(size), source[k].Index(size));
            }
        }
    }
}
