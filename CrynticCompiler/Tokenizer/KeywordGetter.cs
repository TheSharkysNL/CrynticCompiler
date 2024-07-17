using CrynticCompiler.Extensions;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CrynticCompiler.Tokenizer;

public sealed class KeywordGetter<TData> : IReadOnlyDictionary<ReadOnlyMemory<TData>, int>, IDictionary<ReadOnlyMemory<TData>, int>
    where TData : unmanaged, IBinaryNumber<TData>
{
    private readonly Dictionary<ReadOnlyMemory<TData>, int> keywords;

    public KeywordGetter(int capacity = 0)
    {
        keywords = new(capacity, KeywordMemoryComparer.Instance);
    }

    public int this[ReadOnlyMemory<TData> key]
    {
        get => keywords[key];
        set => keywords[key] = value;
    }

    public int Count => keywords.Count;

    public bool IsReadOnly => false;

    public ICollection<ReadOnlyMemory<TData>> Keys => keywords.Keys;

    public ICollection<int> Values => keywords.Values;

    IEnumerable<ReadOnlyMemory<TData>> IReadOnlyDictionary<ReadOnlyMemory<TData>, int>.Keys => keywords.Keys;

    IEnumerable<int> IReadOnlyDictionary<ReadOnlyMemory<TData>, int>.Values => keywords.Values;

    public void Add(ReadOnlyMemory<TData> key, int value) =>
        keywords.Add(key, value);

    public void Add(KeyValuePair<ReadOnlyMemory<TData>, int> item) =>
        keywords.Add(item.Key, item.Value);

    public void Add(ReadOnlySpan<char> key, int value) =>
        Add(key.Convert<char, TData>(), value);

    public void Clear() =>
        keywords.Clear();

    public bool Contains(KeyValuePair<ReadOnlyMemory<TData>, int> item) =>
        keywords.Contains(item);

    public bool Contains(ReadOnlySpan<char> key, int value) =>
        Contains(new(key.Convert<char, TData>(), value));

    public bool ContainsKey(ReadOnlyMemory<TData> key) =>
        keywords.ContainsKey(key);

    public bool ContainsKey(ReadOnlySpan<char> key) =>
        ContainsKey(key.Convert<char, TData>());

    public void CopyTo(KeyValuePair<ReadOnlyMemory<TData>, int>[] array, int arrayIndex) =>
        ((IDictionary<ReadOnlyMemory<TData>, int>)keywords).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<ReadOnlyMemory<TData>, int>> GetEnumerator() =>
        keywords.GetEnumerator();

    public bool Remove(ReadOnlyMemory<TData> key) =>
        keywords.Remove(key);

    public bool Remove(Span<char> key) =>
        Remove(key.Convert<char, TData>());

    public bool Remove(KeyValuePair<ReadOnlyMemory<TData>, int> item) =>
        ((IDictionary<ReadOnlyMemory<TData>, int>)keywords).Remove(item);

    public bool TryGetValue(ReadOnlyMemory<TData> key, [MaybeNullWhen(false)] out int value) =>
        keywords.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() =>
        keywords.GetEnumerator();

    /// <summary>
    /// matches <see cref="ReadOnlyMemory{T}"/> while only including the first whitespace
    /// </summary>
    private sealed class KeywordMemoryComparer : IEqualityComparer<ReadOnlyMemory<TData>>
    {
        public static readonly KeywordMemoryComparer Instance = new();

        private static readonly ReadOnlyMemory<TData> Whitespace = " \t\n\r".AsSpan().Convert<char, TData>();

        private static int WalkOverValues(ReadOnlySpan<TData> span, int start, ReadOnlySpan<TData> values)
        {
            if (start >= span.Length)
                return -1;
            int index = span[start..].IndexOfAnyExcept(values);
            if (index == -1)
                return -1;
            return index + start;
        }

        public bool Equals(ReadOnlyMemory<TData> x, ReadOnlyMemory<TData> y)
        {
            ReadOnlySpan<TData> longestSpan;
            ReadOnlySpan<TData> shortestSpan;

            if (x.Length > y.Length)
            {
                longestSpan = x.Span;
                shortestSpan = y.Span;
            }
            else
            {
                longestSpan = y.Span;
                shortestSpan = x.Span;
            }

            ReadOnlySpan<TData> whitespace = Whitespace.Span;

            int longestLength = longestSpan.Length;
            int shortestLength = shortestSpan.Length;

            int shortestIndex = 0;
            for (int i = 0; i < longestLength;)
            {
                if (shortestIndex >= shortestLength)
                    return false;

                TData longestValue = longestSpan[i];
                TData shortestValue = shortestSpan[shortestIndex];
                if (longestValue != shortestValue)
                    return false;

                int newIndex = i + 1;
                if (whitespace.Contains(longestValue))
                    newIndex = WalkOverValues(longestSpan, newIndex, whitespace);

                int newShortestIndex = shortestIndex + 1;
                if (whitespace.Contains(shortestValue))
                    newShortestIndex = WalkOverValues(shortestSpan, newShortestIndex, whitespace);

                if (newIndex < 0)
                {
                    if (newShortestIndex < 0)
                        return true;
                    return false;
                }
                else if (newShortestIndex < 0)
                    return false;
                shortestIndex = newShortestIndex;
                i = newIndex;
            }

            return true;
        }

        public unsafe int GetHashCode([DisallowNull] ReadOnlyMemory<TData> obj)
        {
            ReadOnlySpan<TData> whitespace = Whitespace.Span;

            int hash = 0x7F7F7F7F;

            bool foundWhitespace = false;

            int dataSize = sizeof(int) / sizeof(TData) == 0 ? 1 : sizeof(int) / sizeof(TData);

            ReadOnlySpan<TData> span = obj.Span;
            int length = obj.Length;
            fixed (TData* ptr = span) {
                for (int i = 0; i < length; i++)
                {
                    ref TData value = ref ptr[i];
                    int offset = i % dataSize * 8 * sizeof(TData);

                    bool newFoundWhitespace = whitespace.Contains(value);

                    if (!(foundWhitespace & newFoundWhitespace))
                    {
                        int val;
                        if (sizeof(TData) == sizeof(byte))
                            val = Unsafe.As<TData, byte>(ref value) * 33;
                        else if (sizeof(TData) == sizeof(short))
                            val = Unsafe.As<TData, short>(ref value) * 33;
                        else if (sizeof(TData) == sizeof(int))
                            val = Unsafe.As<TData, int>(ref value) * 33;
                        else
                            val = Unsafe.As<TData, long>(ref value).GetHashCode() * 33;
                        hash ^= val << offset;
                    }

                    foundWhitespace = newFoundWhitespace;
                }
            }

            return hash;
        }
    }
}
