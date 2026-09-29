namespace RubiksCube.Domain;

public static class ColourExtensions
{
    /// <summary>One letter per colour (W O G R B Y), used in facelet strings and the console.</summary>
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
        _ => throw new FormatException($"'{symbol}' is not a colour symbol (expected W, O, G, R, B or Y)."),
    };
}
