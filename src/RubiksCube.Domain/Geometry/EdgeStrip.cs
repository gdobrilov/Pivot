namespace RubiksCube.Domain.Geometry;

/// <summary>A side of a face and the corner it is read from, e.g. "Up's bottom row, from the bottom-left".</summary>
internal readonly record struct EdgeStrip(Face Face, Side Side, Corner ReadFrom)
{
    /// <summary>The strip's stickers in reading order.</summary>
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

    // Rows read left to right and columns top to bottom; the other corner means backwards.
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
        _ => throw new InvalidOperationException($"Corner {ReadFrom} is not on side {Side}."),
    };
}
