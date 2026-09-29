using System.Collections.Frozen;

namespace RubiksCube.Domain;

/// <summary>Singmaster notation: a face letter, optionally followed by ' (anti-clockwise) or 2 (half turn).</summary>
public static class MoveNotation
{
    private const char AntiClockwiseSuffix = '\'';
    private const char HalfTurnSuffix = '2';

    private static readonly char[] Separators = [' ', '\t', '\r', '\n', ','];

    private static readonly FrozenDictionary<char, Face> FacesBySymbol = new Dictionary<char, Face>
    {
        ['U'] = Face.Up,
        ['L'] = Face.Left,
        ['F'] = Face.Front,
        ['R'] = Face.Right,
        ['B'] = Face.Back,
        ['D'] = Face.Down,
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<Face, char> SymbolsByFace =
        FacesBySymbol.ToFrozenDictionary(pair => pair.Value, pair => pair.Key);

    public static string Format(Move move)
    {
        var symbol = SymbolsByFace[move.Face];
        return move.Rotation switch
        {
            Rotation.Clockwise => symbol.ToString(),
            Rotation.AntiClockwise => string.Concat(symbol, AntiClockwiseSuffix),
            Rotation.Half => string.Concat(symbol, HalfTurnSuffix),
            _ => throw new ArgumentOutOfRangeException(nameof(move), move.Rotation, "Unknown rotation."),
        };
    }

    public static string Format(IEnumerable<Move> moves)
    {
        ArgumentNullException.ThrowIfNull(moves);
        return string.Join(' ', moves.Select(Format));
    }

    /// <summary>Moves are separated by whitespace or commas. On failure, <paramref name="error"/> says which token was wrong.</summary>
    public static bool TryParse(string? notation, out IReadOnlyList<Move> moves, out string? error)
    {
        var parsed = new List<Move>();
        var tokens = (notation ?? string.Empty).Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            if (TryParseToken(token, out var move))
            {
                parsed.Add(move);
                continue;
            }

            moves = [];
            error = $"'{token}' is not a valid move. Expected a face letter (U, L, F, R, B, D) optionally followed by ' or 2.";
            return false;
        }

        moves = parsed;
        error = null;
        return true;
    }

    public static IReadOnlyList<Move> Parse(string notation) =>
        TryParse(notation, out var moves, out var error) ? moves : throw new FormatException(error);

    private static bool TryParseToken(string token, out Move move)
    {
        move = default;
        if (token.Length is < 1 or > 2 || !FacesBySymbol.TryGetValue(token[0], out var face))
        {
            return false;
        }

        if (token.Length == 1)
        {
            move = Move.Clockwise(face);
            return true;
        }

        switch (token[1])
        {
            case AntiClockwiseSuffix:
                move = Move.AntiClockwise(face);
                return true;
            case HalfTurnSuffix:
                move = Move.Half(face);
                return true;
            default:
                return false;
        }
    }
}
