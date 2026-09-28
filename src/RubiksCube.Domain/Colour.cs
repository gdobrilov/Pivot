namespace RubiksCube.Domain;

/// <summary>The six sticker colours of a standard Rubik's cube.</summary>
public enum Colour
{
    White,
    Orange,
    Green,
    Red,
    Blue,
    Yellow,
}

public static class ColourExtensions
{
    /// <summary>Single-letter symbol used in facelet strings and the console view (W, O, G, R, B, Y).</summary>
    public static char ToSymbol(this Colour colour) => colour switch
    {
        Colour.White => 'W',
        Colour.Orange => 'O',
        Colour.Green => 'G',
        Colour.Red => 'R',
        Colour.Blue => 'B',
        Colour.Yellow => 'Y',
        _ => throw new ArgumentOutOfRangeException(nameof(colour), colour, "Unknown colour."),
    };

    public static Colour FromSymbol(char symbol) => symbol switch
    {
        'W' => Colour.White,
        'O' => Colour.Orange,
        'G' => Colour.Green,
        'R' => Colour.Red,
        'B' => Colour.Blue,
        'Y' => Colour.Yellow,
        _ => throw new FormatException($"'{symbol}' is not a colour symbol (expected one of W, O, G, R, B, Y)."),
    };
}
