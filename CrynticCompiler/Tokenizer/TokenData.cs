using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CrynticCompiler.Extensions;

namespace CrynticCompiler.Tokenizer;

public readonly struct TokenData<TData> : ISpanFormattable
    where TData : unmanaged, IBinaryInteger<TData>
{
    private readonly ReadOnlyMemory<TData> data;
    
    public TokenData(TData[] array)
        : this(array, 0, array.Length)
    { }

    public TokenData(TData[] array, int start)
        : this(array, start, array.Length - start)
    { }

    public TokenData(TData[] array, int start, int length)
        : this(new ReadOnlyMemory<TData>(array, start, length))
    { }

    public TokenData(ReadOnlyMemory<TData> memory)
    {
        data = memory;
    }

    public TokenData(TData value)
    {
        if (Unsafe.SizeOf<TData>() > 4)
        {
            throw new ArgumentException($"Cannot initialize TokenData with the type parameter: {nameof(TData)}, it's larger than 4 bytes");
        }
        
        data = GetSingleValue(value);
    }

    public static TokenData<TData> Empty => default;

    public ReadOnlySpan<TData> Span => HasSingleValue()
        ? MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ReadOnlyMemory<TData>, TData>(ref Unsafe.AsRef(in data)), 1)
        : data.Span;

    public int Length => HasSingleValue() ? 1 : data.Length;

    public bool IsEmpty => data.IsEmpty;

    private static ref byte GetByteRef(ref ReadOnlyMemory<TData> memory) =>
        ref Unsafe.As<ReadOnlyMemory<TData>, byte>(ref memory);

    private bool HasSingleValue()
    {
        ReadOnlyMemory<TData> memory = data;
        ref byte byteMemoryRef = ref GetByteRef(ref memory);
        return Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref byteMemoryRef, Unsafe.SizeOf<nuint>())) == -1;
    }

    private ReadOnlyMemory<TData> GetSingleValue(TData value)
    {
        Unsafe.SkipInit(out ReadOnlyMemory<TData> memory);

        ref byte byteMemoryRef = ref Unsafe.As<ReadOnlyMemory<TData>, byte>(ref memory);
        
        Unsafe.WriteUnaligned(ref byteMemoryRef, value);
        Unsafe.WriteUnaligned<int>(ref Unsafe.Add(ref byteMemoryRef, Unsafe.SizeOf<nuint>()), -1);
        return memory;
    }

    private static void Format(ReadOnlySpan<TData> source, Span<char> destination, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<char>())
        {
            ReadOnlySpan<char> charSourceSpan = MemoryMarshal.Cast<TData, char>(source);
            charSourceSpan.CopyTo(destination);
            return;
        }
        
        int sourceLength = source.Length;
        for (int i = 0; i < sourceLength; i++)
        {
            destination[i] = (char)ushort.CreateTruncating(source[i]);
        }
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        ReadOnlySpan<TData> span = Span;
        if (destination.Length < span.Length)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = span.Length;
        Format(span, destination, format, provider);
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<char>())
        {
            return Span.ToString();
        }

        ReadOnlySpan<TData> span = Span;
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return string.Empty;
        }
        
        string str = new string('\0', spanLength);

        ref char strRef = ref Unsafe.AsRef(in str.GetPinnableReference());
        Span<char> strSpan = MemoryMarshal.CreateSpan(ref strRef, spanLength);

        Format(span, strSpan, default, null);
        return str;
    }

    public override string ToString() =>
        Span.ConvertToCharacters();
}