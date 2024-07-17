using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrynticCompiler.Extensions;

public static partial class MemoryExtensions
{
    public static unsafe string ConvertToCharacters<T>(this Span<T> span)
        where T : unmanaged =>
        ((ReadOnlySpan<T>)span).ConvertToCharacters();

    public static unsafe void ConvertToCharacters<T>(this Span<T> span, Span<char> converted)
        where T : unmanaged =>
        ((ReadOnlySpan<T>)span).ConvertToCharacters(converted);

    public static unsafe string ConvertToCharacters<T>(this ReadOnlySpan<T> span)
        where T : unmanaged
    {
        int length = span.Length;
        string convert = new('\0', length);

        fixed (char* p = convert)
            span.ConvertToCharacters(new(p, length));

        return convert;
    }

    public static unsafe void ConvertToCharacters<T>(this ReadOnlySpan<T> span, Span<char> converted)
        where T : unmanaged
    {
        int length = span.Length;

        fixed (char* convertPtr = converted)
        fixed (T* spanPtr = span)
        {
            char* currPtr = convertPtr;
            for (int i = 0; i < length; i++)
            {
                char c;
                if (sizeof(T) == sizeof(byte))
                    c = (char)Unsafe.As<T, byte>(ref spanPtr[i]);
                else
                    c = Unsafe.As<T, char>(ref spanPtr[i]);
                *(currPtr + i) = c;
            }
        }
    }

    public static T[] Convert<TSource, T>(this Memory<TSource> memory)
        where T : unmanaged
        where TSource : unmanaged =>
        Convert<TSource, T>(memory.Span);

    public static T[] Convert<TSource, T>(this ReadOnlyMemory<TSource> memory)
        where T : unmanaged
        where TSource : unmanaged =>
        Convert<TSource, T>(memory.Span);


    public static  T[] Convert<TSource, T>(this Span<TSource> span)
        where T : unmanaged
        where TSource : unmanaged =>
        Convert<TSource, T>(span);

    public static T[] Convert<TSource, T>(this ReadOnlySpan<TSource> span)
        where T : unmanaged
        where TSource : unmanaged
    {
        int spanLength = span.Length;

        T[] arr = new T[spanLength];

        ref T dest = ref MemoryMarshal.GetArrayDataReference(arr);
        ref TSource source = ref MemoryMarshal.GetReference(span);

        ref TSource end = ref Unsafe.Add(ref source, spanLength);

        while (Unsafe.IsAddressLessThan(ref source, ref end))
        {
            if (Unsafe.SizeOf<T>() < Unsafe.SizeOf<TSource>())
                dest = Unsafe.As<TSource, T>(ref source);
            else
                Unsafe.As<T, TSource>(ref dest) = source;

            source = ref Unsafe.Add(ref source, 1);
            dest = ref Unsafe.Add(ref dest, 1);
        }

        return arr;
    }
}