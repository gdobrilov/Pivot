using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace RubiksCube.Domain.Geometry;

/// <summary>
/// How the faces are glued together. For each face: the four neighbouring strips a clockwise turn
/// drags round, in clockwise order, each read from the corner met first (so sticker k lands on sticker k).
/// </summary>
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

    /// <summary>Built once per (size, move) and cached.</summary>
    public static Permutation PermutationFor(Move move, int size) =>
        Cache.GetOrAdd((size, move), key => Build(key.Move, key.Size));

    /// <summary>For tests.</summary>
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

    // The face itself: (row, column) moves to (column, size − 1 − row).
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

    // Each strip moves onto the next one, sticker k onto sticker k.
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
