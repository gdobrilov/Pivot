namespace RubiksCube.Domain.Geometry;

/// <summary>
/// A rearrangement of the cube's stickers: after applying it, the sticker at index <c>i</c> is the
/// one that was at index <c>this[i]</c> before. Positions not mentioned map to themselves.
/// </summary>
internal sealed class Permutation
{
    private readonly int[] _source;

    private Permutation(int[] source) => _source = source;

    public int Length => _source.Length;

    public int this[int index] => _source[index];

    public static Permutation Identity(int length) => new(Enumerable.Range(0, length).ToArray());

    /// <summary>Builds a permutation from (target, source) pairs; everything else stays in place.</summary>
    public static Permutation FromMappings(int length, IEnumerable<(int Target, int Source)> mappings)
    {
        var source = Enumerable.Range(0, length).ToArray();
        foreach (var (target, origin) in mappings)
        {
            source[target] = origin;
        }

        return new Permutation(source);
    }

    /// <summary>The permutation that undoes this one.</summary>
    public Permutation Inverse()
    {
        var inverse = new int[_source.Length];
        for (var i = 0; i < _source.Length; i++)
        {
            inverse[_source[i]] = i;
        }

        return new Permutation(inverse);
    }

    /// <summary>This permutation followed by <paramref name="next"/>.</summary>
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
