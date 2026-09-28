namespace RubiksCube.Domain.Geometry;

/// <summary>
/// One side of a face together with the corner it is read from, e.g. "the bottom row of Up,
/// read from its bottom-left corner". Reading direction matters: when a neighbouring face turns,
/// sticker k of one strip lands on sticker k of the next.
/// </summary>
internal readonly record struct EdgeStrip(Face Face, Side Side, Corner ReadFrom)
{
    /// <summary>The positions of the strip's stickers, in reading order, for a cube of the given size.</summary>
    public IReadOnlyList<StickerPosition> Positions(int size)
    {
        var face = Face;
        var last = size - 1;
        var positions = Side switch
        {
            Side.Top => Enumerable.Range(0, size).Select(column => new StickerPosition(face, 0, column)),
            Side.Bottom => Enumerable.Range(0, size).Select(column => new StickerPosition(face, last, column)),
            Side.Left => Enumerable.Range(0, size).Select(row => new StickerPosition(face, row, 0)),
            Side.Right => Enumerable.Range(0, size).Select(row => new StickerPosition(face, row, last)),
            _ => throw new InvalidOperationException($"Unknown side {Side}."),
        };

        return ReadsBackwards ? positions.Reverse().ToArray() : positions.ToArray();
    }

    /// <summary>
    /// Rows are naturally read left to right and columns top to bottom; starting from the
    /// opposite corner means reading the strip backwards.
    /// </summary>
    private bool ReadsBackwards => (Side, ReadFrom) switch
    {
        (Side.Top, Corner.TopLeft) => false,
        (Side.Top, Corner.TopRight) => true,
        (Side.Bottom, Corner.BottomLeft) => false,
        (Side.Bottom, Corner.BottomRight) => true,
        (Side.Left, Corner.TopLeft) => false,
        (Side.Left, Corner.BottomLeft) => true,
        (Side.Right, Corner.TopRight) => false,
        (Side.Right, Corner.BottomRight) => true,
        _ => throw new InvalidOperationException($"Corner {ReadFrom} does not lie on side {Side}."),
    };
}
