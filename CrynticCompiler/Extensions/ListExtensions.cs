using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrynticCompiler.Extensions;

public static class ListExtensions
{
    public static void CopyTo<T>(this IReadOnlyList<T> list, Span<T> span) =>
        CopyToInternal(list, 0, span, 0, span.Length);

    public static void CopyTo<T>(this IReadOnlyList<T> list, int listIndex, Span<T> span) =>
        CopyTo(list, listIndex, span, 0, span.Length);

    public static void CopyTo<T>(this IReadOnlyList<T> list, Span<T> span, int spanIndex) =>
        CopyTo(list, span, spanIndex, span.Length - spanIndex);

    public static void CopyTo<T>(this IReadOnlyList<T> list, Span<T> span, int spanIndex, int spanLength) =>
        CopyTo(list, 0, span, spanIndex, spanLength);

    public static void CopyTo<T>(this IReadOnlyList<T> list, int listIndex, Span<T> span, int spanIndex) =>
        CopyTo(list, listIndex, span, spanIndex, span.Length - spanIndex);

    public static void CopyTo<T>(this IReadOnlyList<T> list, int listIndex, Span<T> span, int spanIndex, int spanLength)
	{
        if ((uint)listIndex >= list.Count)
            throw new ArgumentOutOfRangeException(nameof(listIndex));
        if (spanIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(spanIndex));
        if ((uint)(span.Length - spanIndex) < spanLength)
            throw new ArgumentOutOfRangeException(nameof(spanLength));

        CopyToInternal(list, listIndex, span, spanIndex, spanLength);
    }

    private static void CopyToInternal<T>(this IReadOnlyList<T> list, int listIndex, Span<T> span, int spanIndex, int spanLength)
    {
        Debug.Assert((uint)listIndex < list.Count);
        Debug.Assert(spanIndex >= 0);
        Debug.Assert((uint)(span.Length - spanIndex) >= spanLength);

        if (list is T[] arr)
            arr.AsSpan(listIndex).CopyTo(span.Slice(spanIndex, spanLength));
        else if (list is List<T> l)
            CollectionsMarshal.AsSpan(l).Slice(listIndex).CopyTo(span.Slice(spanIndex, spanLength));
        else
        {
            ref T spanRef = ref MemoryMarshal.GetReference(span);
            spanRef = ref Unsafe.Add(ref spanRef, spanIndex);

            for (int i = 0; i < spanLength; i++)
            {
                T source = list[i + listIndex];
                ref T destination = ref Unsafe.Add(ref spanRef, i);
                destination = source;
            }
        }
    }
}

