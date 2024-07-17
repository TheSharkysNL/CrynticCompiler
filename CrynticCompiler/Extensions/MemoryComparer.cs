using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CrynticCompiler.Extensions;

public sealed class MemoryComparer<TData> : IEqualityComparer<Memory<TData>>
    where TData : unmanaged
{
    public static readonly MemoryComparer<TData> Instance = new();

    public bool Equals(Memory<TData> x, Memory<TData> y) =>
        SpanComparer<TData>.Equals(x.Span, y.Span);

    public unsafe int GetHashCode([DisallowNull] Memory<TData> obj) =>
        SpanComparer<TData>.GetHashCode(obj.Span);
}

public sealed class ReadOnlyMemoryComparer<TData> : IEqualityComparer<ReadOnlyMemory<TData>>
    where TData : unmanaged
{
    public static readonly ReadOnlyMemoryComparer<TData> Instance = new();

    public bool Equals(ReadOnlyMemory<TData> x, ReadOnlyMemory<TData> y) =>
        SpanComparer<TData>.Equals(x.Span, y.Span);

    public unsafe int GetHashCode([DisallowNull] ReadOnlyMemory<TData> obj) =>
        SpanComparer<TData>.GetHashCode(obj.Span);
}

file static class SpanComparer<TData>
    where TData : unmanaged
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(ReadOnlySpan<TData> x, ReadOnlySpan<TData> y) =>
        x.SequenceEqual(y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int GetHashCode([DisallowNull] ReadOnlySpan<TData> span)
    {
        nuint hash = unchecked((nuint)0x7F7F7F7F7F7F7F7FUL);

        (int length, int remainder) = int.DivRem(span.Length * sizeof(TData), sizeof(nuint));
        fixed (TData* ptr = span)
        {
            byte* currPtr = (byte*)ptr;
            byte* endPtr = (byte*)ptr + length;
            int increaseAmount = sizeof(nuint) / sizeof(TData) == 0 ? 1 : sizeof(nuint) / sizeof(TData);
            for (; currPtr < endPtr; currPtr += increaseAmount)
            {
                nuint val = *(nuint*)currPtr;
                hash ^= val * 33;
            }
            if (Environment.Is64BitProcess && remainder >= sizeof(int))
            {
                int val = *(int*)currPtr;
                hash ^= (nuint)val * 33;
                remainder -= sizeof(int);
                currPtr += sizeof(int);
            }
            if (remainder >= sizeof(short))
            {
                short val = *(short*)currPtr;
                hash ^= (nuint)val * 33;
                remainder -= sizeof(short);
                currPtr += sizeof(short);
            }
            if (remainder >= sizeof(byte))
            {
                byte val = *(byte*)currPtr;
                hash ^= (nuint)val * 33;
            }
        }

        if (Environment.Is64BitProcess)
            return HashCode.Combine((int)hash, (int)(hash >> 32));
        return (int)hash;
    }
}