using System.Runtime.CompilerServices;

namespace CrynticCompiler.Extensions;

public static partial class MemoryExtensions
{
    /// <summary>
    /// gets the index of <paramref name="value"/> at <paramref name="start"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span">the span to search in</param>
    /// <param name="value">the value to search for</param>
    /// <param name="start">the position to start at</param>
    /// <param name="index">the index of the <paramref name="value"/></param>
    /// <returns>true if the index was found els false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, int start, out int index)
        where T : IEquatable<T>
    {
        int i = span.Slice(start).IndexOf(value);
        index = i + start;
        return i != -1;
    }

    /// <summary>
    /// gets the index of <paramref name="value"/> at <paramref name="start"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span">the span to search in</param>
    /// <param name="value">the value to search for</param>
    /// <param name="start">the position to start at</param>
    /// <param name="index">the index of the <paramref name="value"/></param>
    /// <returns>true if the index was found els false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IndexOf<T>(this ReadOnlySpan<T> span, T value, int start, out int index)
        where T : IEquatable<T>
    {
        int i = span.Slice(start).IndexOf(value);
        index = i + start;
        return i != -1;
    }
    
    /// <summary>
    /// gets the index of <paramref name="value"/> at <paramref name="start"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span">the span to search in</param>
    /// <param name="value">the value to search for</param>
    /// <param name="start">the position to start at</param>
    /// <param name="index">the index of the <paramref name="value"/></param>
    /// <returns>true if the index was found els false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IndexOf<T>(this Span<T> span, T value, int start, out int index)
        where T : IEquatable<T>
    {
        int i = span.Slice(start).IndexOf(value);
        index = i + start;
        return i != -1;
    }

    /// <summary>
    /// gets the index of <paramref name="value"/> or <paramref name="value2"/> at <paramref name="start"/> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span">the span to search in</param>
    /// <param name="value">the value to search for</param>
    /// <param name="value2">the second value to search for</param>
    /// <param name="start">the position to start at</param>
    /// <param name="index">the index of the <paramref name="value"/></param>
    /// <returns>true if the index was found els false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IndexOfAny<T>(this ReadOnlySpan<T> span, T value, T value2, int start, out int index)
        where T : IEquatable<T>
    {
        int i = span.Slice(start).IndexOfAny(value, value2);
        index = i + start;
        return i != -1;
    }
}
