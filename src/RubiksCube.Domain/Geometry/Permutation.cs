namespace RubiksCube.Domain.Geometry;

/// <summary>After applying it, position i holds what was at position this[i].</summary>
internal sealed class Permutation
{
    private readonly int[] _source;

    private Permutation(int[] source) => _source = source;

    public int this[int index] => _source[index];

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

    public void Apply<T>(ReadOnlySpan<T> source, Span<T> target)
    {
        for (var i = 0; i < _source.Length; i++)
        {
            target[i] = source[_source[i]];
        }
    }
}
