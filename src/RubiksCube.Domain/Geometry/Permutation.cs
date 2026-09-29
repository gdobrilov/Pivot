namespace RubiksCube.Domain.Geometry;

/// <summary>After applying it, position i holds what was at position source[i].</summary>
internal sealed class Permutation
{
    private readonly int[] _source;

    private Permutation(int[] source) => _source = source;

    /// <summary>Positions not listed stay where they are.</summary>
    public static Permutation FromMappings(int length, IEnumerable<(int Target, int Source)> mappings)
    {
        var source = Enumerable.Range(0, length).ToArray();
        foreach (var (target, origin) in mappings)
        {
            source[target] = origin;
        }

        return new Permutation(source);
    }

    public Permutation Inverse()
    {
        var inverse = new int[_source.Length];
        for (var i = 0; i < _source.Length; i++)
        {
            inverse[_source[i]] = i;
        }

        return new Permutation(inverse);
    }

    /// <summary>This one, then <paramref name="next"/>.</summary>
    public Permutation Then(Permutation next)
    {
        var composed = new int[_source.Length];
        for (var i = 0; i < _source.Length; i++)
        {
            composed[i] = _source[next._source[i]];
        }

        return new Permutation(composed);
    }

    public Colour[] Apply(Colour[] stickers)
    {
        var result = new Colour[stickers.Length];
        for (var i = 0; i < _source.Length; i++)
        {
            result[i] = stickers[_source[i]];
        }

        return result;
    }

    /// <summary>True when every position appears exactly once as a source. For tests.</summary>
    internal bool IsBijection() => _source.Order().SequenceEqual(Enumerable.Range(0, _source.Length));
}
