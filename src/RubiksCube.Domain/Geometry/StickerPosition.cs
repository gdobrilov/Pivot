namespace RubiksCube.Domain.Geometry;

/// <summary>Address of one sticker. <see cref="Index"/> is the only place that maps it to the flat array.</summary>
internal readonly record struct StickerPosition(Face Face, int Row, int Column)
{
    public int Index(int size) => ((int)Face * size * size) + (Row * size) + Column;
}
