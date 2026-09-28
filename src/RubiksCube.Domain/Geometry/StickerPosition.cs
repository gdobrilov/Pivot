namespace RubiksCube.Domain.Geometry;

/// <summary>Identifies one sticker by face, row and column.</summary>
internal readonly record struct StickerPosition(Face Face, int Row, int Column)
{
    /// <summary>Index into the cube's flat sticker array for a cube of the given size.</summary>
    public int Index(int size) => ((int)Face * size * size) + (Row * size) + Column;
}
