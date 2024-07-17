using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace CrynticCompiler.Extensions;

public static partial class MemoryExtensions
{
    public static int Count<T>(this ReadOnlyMemory<T> memory, T value) where T : IEquatable<T> =>
        Count(memory.Span, value);

    public static unsafe int Count<T>(this ReadOnlySpan<T> span, T value)
        where T : IEquatable<T>
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            fixed (T* ptr = span)
                if (sizeof(T) <= sizeof(ulong))
                    return CountNumberBaseConversion(ptr, (nuint)span.Length, value);
        return CountIEquatable(span, value);
    }

    public static int Count<T>(this ReadOnlyMemory<T> memory, ReadOnlySpan<T> value) where T : IEquatable<T> =>
        Count(memory.Span, value);

    public static unsafe int Count<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value)
        where T : IEquatable<T>
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            fixed (T* ptr = span)
                if (sizeof(T) * value.Length <= sizeof(ulong))
                    return CountNumberBaseConversion(ptr, (nuint)span.Length, value);
        return CountIEquatable(span, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountNumberBaseConversion<T>(void* src, nuint length, T value)
    {
        Debug.Assert(sizeof(T) <= sizeof(ulong));
        Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<T>());

        bool supportsPrefetch =
            Sse.IsSupported && Vector.IsHardwareAccelerated &&
            (
                (Sse2.IsSupported && Vector128.IsHardwareAccelerated) ||
                (Avx.IsSupported && Vector256.IsHardwareAccelerated)
            );
        if (sizeof(T) == sizeof(byte))
        {
            if (supportsPrefetch)
                return CountSsePrefetch(src, length, Unsafe.As<T, byte>(ref value));
            return CountVectorized(src, length, Unsafe.As<T, byte>(ref value));
        }
        else if (sizeof(T) == sizeof(ushort))
        {
            if (supportsPrefetch)
                return CountSsePrefetch(src, length, Unsafe.As<T, ushort>(ref value));
            return CountVectorized(src, length, Unsafe.As<T, ushort>(ref value));
        }
        else if (sizeof(T) == sizeof(uint))
        {
            if (supportsPrefetch)
                return CountSsePrefetch(src, length, Unsafe.As<T, ushort>(ref value));
            return CountVectorized(src, length, Unsafe.As<T, ushort>(ref value));
        }
        if (supportsPrefetch)
            return CountSsePrefetch(src, length, Unsafe.As<T, ushort>(ref value));
        return CountVectorized(src, length, Unsafe.As<T, ushort>(ref value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountNumberBaseConversion<T>(void* src, nuint length, ReadOnlySpan<T> value)
    {
        Debug.Assert(sizeof(T) * value.Length <= sizeof(ulong));
        Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<T>());

        bool supportsPrefetch =
            Sse.IsSupported && Vector.IsHardwareAccelerated &&
            (
                (Sse2.IsSupported && Vector128.IsHardwareAccelerated) ||
                (Avx.IsSupported && Vector256.IsHardwareAccelerated)
            );

        fixed (T* ptr = value)
        {
            nuint size = (nuint)sizeof(T) * length;
            if (size == sizeof(byte))
            {
                if (supportsPrefetch)
                    return CountSsePrefetch(src, length, *(byte*)ptr);
                return CountVectorized(src, length, *(byte*)ptr);
            }
            else if (size == sizeof(ushort))
            {
                if (supportsPrefetch)
                    return CountSsePrefetch(src, length, *(ushort*)ptr);
                return CountVectorized(src, length, *(ushort*)ptr);
            }
            else if (size == sizeof(uint))
            {
                if (supportsPrefetch)
                    return CountSsePrefetch(src, length, *(uint*)ptr);
                return CountVectorized(src, length, *(uint*)ptr);
            }
            if (supportsPrefetch)
                return CountSsePrefetch(src, length, *(ulong*)ptr);
            return CountVectorized(src, length, *(ulong*)ptr);
        }
    }



    private static int CountIEquatable<T>(ReadOnlySpan<T> span, T value)
        where T : IEquatable<T>
    {
        int count = 0;
        for (int i = 0; i < span.Length; i++)
            if (span[i].Equals(value))
                count++;
        return count;
    }

    private static int CountIEquatable<T>(ReadOnlySpan<T> span, ReadOnlySpan<T> value)
        where T : IEquatable<T>
    {
        int count = 0;
        int length = value.Length;
        for (int i = 0; i < span.Length; i++)
            if (span.Slice(i, length).SequenceEqual(value))
                count++;
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong HasZero<T>(ulong value)
        where T : unmanaged
    {
        Debug.Assert(sizeof(T) <= sizeof(uint));
        if (sizeof(T) == sizeof(byte))
            return ((value) - 0x0101010101010101UL) & ~(value) & 0x8080808080808080UL;
        else if (sizeof(T) == sizeof(short))
            return ((value) - 0x0001000100010001UL) & ~(value) & 0x0080008000800080UL;
        else
            return ((value) - 0x0000000100000001UL) & ~(value) & 0x0000008000000080UL;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint HasZero<T>(uint value)
        where T : unmanaged
    {
        Debug.Assert(sizeof(T) <= sizeof(ushort));
        if (sizeof(T) == sizeof(byte))
            return (value) - 0x01010101U & ~(value) & 0x80808080U;
        else
            return (value) - 0x00010001U & ~(value) & 0x00800080U;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong HasValue<T>(ulong value, T b)
        where T : unmanaged, INumberBase<T>
    {
        Debug.Assert(sizeof(T) <= sizeof(uint));
        return HasZero<T>((value) ^ (~0UL / ((1U << sizeof(T) * 8) - 1) * ulong.CreateTruncating(b)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint HasValue<T>(uint value, T b)
        where T : unmanaged, INumberBase<T>
    {
        Debug.Assert(sizeof(T) <= sizeof(ushort));
        return HasZero<T>((value) ^ (~0U / ((1U << sizeof(T) * 8) - 1) * uint.CreateTruncating(b)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint Count<T>(uint value, T b)
        where T : unmanaged, INumberBase<T>
    {
        Debug.Assert(sizeof(T) <= sizeof(ushort));
        if (Popcnt.IsSupported)
            return Popcnt.PopCount(HasValue(value, b));
        return ((HasValue(value, b) >> 7) * 0x01010101) >> 24;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong Count<T>(ulong value, T b)
        where T : unmanaged, INumberBase<T>
    {
        Debug.Assert(sizeof(T) <= sizeof(uint));
        if (Popcnt.X64.IsSupported)
            return Popcnt.X64.PopCount(HasValue(value, b));
        return ((HasValue(value, b) >> 7) * 0x0101010101010101UL) >> 56;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountOffset<T>(void* src, nuint offset, T value)
        where T : unmanaged, INumberBase<T>
    {
        Debug.Assert(Vector.IsHardwareAccelerated);
        if (Environment.Is64BitProcess)
        {
            if (Vector<byte>.Count == 32)
            {
                if (offset == 24)
                {
                    ulong count = Count(*(ulong*)src, value);
                    count += Count(*((ulong*)src + 1), value);
                    count += Count(*((ulong*)src + 2), value);
                    return (int)count;
                }
                else if (offset == 16)
                {
                    ulong count = Count(*(ulong*)src, value);
                    count += Count(*((ulong*)src + 1), value);
                    return (int)count;
                }
                else
                {
                    ulong count = Count(*(ulong*)src, value);
                    return (int)count;
                }
            }
            else if (Vector<byte>.Count == 16)
            {
                if (offset == 8)
                {
                    ulong count = Count(*(ulong*)src, value);
                    return (int)count;
                }
            }
        }
        else
        {
            if (Vector<byte>.Count == 32)
            {
                if (offset == 28)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    count += Count(*((uint*)src + 2), value);
                    count += Count(*((uint*)src + 3), value);
                    count += Count(*((uint*)src + 4), value);
                    count += Count(*((uint*)src + 5), value);
                    count += Count(*((uint*)src + 6), value);
                    return (int)count;
                }
                if (offset == 24)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    count += Count(*((uint*)src + 2), value);
                    count += Count(*((uint*)src + 3), value);
                    count += Count(*((uint*)src + 4), value);
                    count += Count(*((uint*)src + 5), value);
                    return (int)count;
                }
                else if (offset == 20)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    count += Count(*((uint*)src + 2), value);
                    count += Count(*((uint*)src + 3), value);
                    count += Count(*((uint*)src + 4), value);
                    return (int)count;
                }
                else if (offset == 16)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    count += Count(*((uint*)src + 2), value);
                    count += Count(*((uint*)src + 3), value);
                    return (int)count;
                }
                else if (offset == 12)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    count += Count(*((uint*)src + 2), value);
                    return (int)count;
                }
                else if (offset == 8)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    return (int)count;
                }
                else
                {
                    uint count = Count(*(uint*)src, value);
                    return (int)count;
                }
            }
            else if (Vector<byte>.Count == 16)
            {
                if (offset == 12)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    count += Count(*((uint*)src + 2), value);
                    return (int)count;
                }
                else if (offset == 8)
                {
                    uint count = Count(*(uint*)src, value);
                    count += Count(*((uint*)src + 1), value);
                    return (int)count;
                }
                else
                {
                    uint count = Count(*(uint*)src, value);
                    return (int)count;
                }
            }
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountRemainder<T>(void* src, nuint remainder, T value)
        where T : unmanaged, INumberBase<T>
    {
        int count = 0;
        if (Vector256.IsHardwareAccelerated)
        {
            if (remainder >= 16)
            {
                if (Environment.Is64BitProcess)
                {
                    count += (int)Count(*(ulong*)src, value);
                    count += (int)Count(*((ulong*)src + 1), value);
                }
                else
                {
                    count += (int)Count(*(uint*)src, value);
                    count += (int)Count(*((uint*)src + 1), value);
                    count += (int)Count(*((uint*)src + 2), value);
                    count += (int)Count(*((uint*)src + 3), value);
                }

                remainder -= 16;
                src = (byte*)src + 16;
            }
        }
        if (remainder >= 8)
        {
            if (Environment.Is64BitProcess)
            {
                if (sizeof(T) == sizeof(long))
                {
                    bool eq = *(ulong*)src == Unsafe.As<T, ulong>(ref value);
                    count += Unsafe.As<bool, byte>(ref eq);
                    return count;
                }
                count += (int)Count(*(ulong*)src, value);
            }
            else
            {
                if (sizeof(T) == sizeof(long))
                {
                    bool eq = *(uint*)src == Unsafe.As<T, uint>(ref value) & *((uint*)src + 1) == *((uint*)&value + 1);
                    count += Unsafe.As<bool, byte>(ref eq);
                    return count;
                }
                count += (int)Count(*(uint*)src, value);
                count += (int)Count(*((uint*)src + 1), value);
            }

            src = (byte*)src + 8;
            remainder += 8;
        }
        if (sizeof(T) <= 4)
        {
            if (remainder >= 4)
            {
                if (sizeof(T) == sizeof(uint))
                {
                    bool eq = *(uint*)src == Unsafe.As<T, uint>(ref value);
                    count += Unsafe.As<bool, byte>(ref eq);
                    return count;
                }
                count += (int)Count(*(uint*)src, value);
                src = (byte*)src + 4;
                remainder += 4;
            }
        }
        if (sizeof(T) <= 2)
        {
            if (remainder >= 2)
            {
                if (sizeof(T) == sizeof(short))
                {
                    bool eq = *(short*)src == Unsafe.As<T, short>(ref value);
                    count += Unsafe.As<bool, byte>(ref eq);
                    return count;
                }
                count += (int)Count(*(ushort*)src, value);
                src = (byte*)src + 2;
                remainder += 2;
            }
        }
        if (sizeof(T) <= 1)
        {
            if (remainder >= 1)
            {
                bool eq = *(byte*)src == Unsafe.As<T, byte>(ref value);
                count += Unsafe.As<bool, byte>(ref eq);
            }
        }
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<T> Create<T>(T value)
        where T : struct
    {
        Debug.Assert(Vector.IsHardwareAccelerated);
        if (Vector256.IsHardwareAccelerated)
            return Vector256.Create(value).AsVector();
        else
            return Vector128.Create(value).AsVector();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector<T> LoadAligned<T>(T* ptr)
        where T : unmanaged
    {
        Debug.Assert(Vector.IsHardwareAccelerated);
        if (Vector256.IsHardwareAccelerated)
            return Vector256.LoadAligned(ptr).AsVector();
        else
            return Vector128.LoadAligned(ptr).AsVector();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector<T> Load<T>(T* ptr)
        where T : unmanaged
    {
        Debug.Assert(Vector.IsHardwareAccelerated);
        if (Vector256.IsHardwareAccelerated)
            return Vector256.Load(ptr).AsVector();
        else
            return Vector128.Load(ptr).AsVector();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ExtractMostSignificantBits<T>(Vector<T> vector)
        where T : struct
    {
        Debug.Assert(Vector.IsHardwareAccelerated);
        if (Vector256.IsHardwareAccelerated)
            return vector.AsVector256().ExtractMostSignificantBits();
        else
            return vector.AsVector128().ExtractMostSignificantBits();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Count<T>(Vector<T> vector, Vector<T> valueVec)
        where T : struct
    {
        Vector<T> eq = Vector.Equals(vector, valueVec);

        return BitOperations.PopCount(ExtractMostSignificantBits(eq));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountLoadAligned<T>(T* ptr, Vector<T> valueVec)
        where T : unmanaged =>
        Count(LoadAligned(ptr), valueVec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountLoad<T>(T* ptr, Vector<T> valueVec)
        where T : unmanaged =>
        Count(Load(ptr), valueVec);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountSsePrefetch<T>(void* src, nuint length, T value)
        where T : unmanaged, INumberBase<T>
    {
        Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        Debug.Assert(length >= 0);
        Debug.Assert(Sse.IsSupported);
        Debug.Assert(Vector.IsHardwareAccelerated);
        Debug.Assert(
            (Sse2.IsSupported && Vector128.IsHardwareAccelerated) ||
            (Avx.IsSupported && Vector256.IsHardwareAccelerated)
        );

        nuint offset = (nuint)Vector<byte>.Count - (nuint)src % (nuint)Vector<byte>.Count;

        int count = 0;
        if (offset != (nuint)Vector<byte>.Count)
        {
            if (length < offset)
                return CountRemainder(src, length, value);

            count = CountRemainder(src, offset, value);
        }
        length *= (nuint)sizeof(T);
        length -= offset;

        (nuint loops, nuint remainder) = nuint.DivRem(length, (nuint)Vector<byte>.Count);
        Vector<T>* start = (Vector<T>*)((byte*)src + offset);
        Vector<T>* end = start + loops - 1;


        Vector<T> valueVec = Create(value);
        while (start < end)
        {
            Vector<T>* next = start + 2;
            //Sse.Prefetch0(next); // doesn't make it faster for me

            count += CountLoadAligned((T*)start, valueVec);
            count += CountLoadAligned((T*)(start + 1), valueVec);
            start = next;
        }

        if ((loops & 1) != 0)
        {
            count += CountLoadAligned((T*)start, valueVec);
            start++;
        }

        if (remainder != 0)
            count += CountRemainder(start, remainder, value);

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int CountVectorized<T>(void* src, nuint length, T value)
        where T : unmanaged, INumberBase<T>
    {
        Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        Debug.Assert(length >= 0);

        length *= (nuint)sizeof(T);

        (nuint loops, nuint remainder) = nuint.DivRem(length, (nuint)Vector<byte>.Count);
        Vector<T>* start = (Vector<T>*)src;
        Vector<T>* end = start + loops - 1;

        int count = 0;
        Vector<T> valueVec = Create(value);
        while (start < end)
        {
            count += CountLoad((T*)start, valueVec);
            count += CountLoad((T*)(start + 1), valueVec);
            start += 2;
        }

        if ((loops & 1) != 0)
        {
            count += CountLoad((T*)start, valueVec);
            start++;
        }

        if (remainder == 0)
            count += CountRemainder(start, remainder, value);

        return count;
    }
}