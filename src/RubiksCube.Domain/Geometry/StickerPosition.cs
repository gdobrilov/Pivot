namespace RubiksCube.Domain.Geometry;

/// <summary>Address of one sticker; <see cref="Index"/> is its place in the cube's flat array.</summary>
internal readonly record struct StickerPosition(Face Face, int Row, int Column)
{
    public int Index(int size) => ((int)Face * size * size) + (Row * size) + Column;
}
