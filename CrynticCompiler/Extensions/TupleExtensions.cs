using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrynticCompiler.Extensions;

[DebuggerDisplay("{DebuggerDisplay, nq}")]
public readonly ref struct TupleSpan<T>
	where T : unmanaged
{
	private readonly Span<T> span;
	public readonly int Length;

	internal TupleSpan(Span<T> tupleSpan, int tupleLength)
	{
		span = tupleSpan;
		Length = tupleLength;
    }

	public T this[int index]
	{
		get
		{
			int offset = index / 8;
            int tupleIndex = index - offset;
            index += offset;
			if (tupleIndex >= Length)
				throw new IndexOutOfRangeException();

			return span[index];
		}
		set
		{
            int offset = index / 8;
            int tupleIndex = index - offset;
            index += offset;
            if (tupleIndex >= Length)
                throw new IndexOutOfRangeException();

            span[index] = value;
        }
    }

#if DEBUG
	private string DebuggerDisplay
	{
		get
		{
            Span<T> span = this.span;
            int length = span.Length;
			int tupleLength = Length;
			if (length == 0)
				return "()";

            ValueListBuilder<char> builder = new(stackalloc char[512]);
			builder.Append('(');

			builder.Append(span[0].ToString());
			int charactersAdded = 1;
			for (int i = 1; i < length; i++)
			{
				if ((i + 1) % 8 == 0)
					continue;
				builder.Append(", ");
				builder.Append(span[i].ToString());
				charactersAdded++;
				if (charactersAdded == tupleLength)
					break;
			}

			builder.Append(')');

			ReadOnlySpan<char> chars = builder.AsSpan();
			string str = new(chars);
			builder.Dispose();
			return str;
		}
	}
#endif
}

public static class TupleExtensions
{
	public static bool TryConvertTupleSpan<TTuple, T>(this TTuple tuple, out TupleSpan<T> span)  
		where TTuple : unmanaged, ITuple
		where T : unmanaged
    {
        /*
		 cannot use UnsafeTryConvertTupleSpan, this will create a copy of the tuple
		 and then remove that copy when the UnsafeTryConvertTupleSpan function gets popped off the stack
		 which means the span will hold unknown values
		 */
        Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<TTuple>());
        if (Unsafe.SizeOf<T>() != Unsafe.SizeOf<TTuple>() / tuple.Length)
        {
            span = default;
            return false;
        }
        if (Unsafe.SizeOf<TTuple>() % Unsafe.SizeOf<T>() != 0)
        {
            span = default;
            return false;
        }

        Span<byte> byteSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<TTuple, byte>(ref Unsafe.AsRef(tuple)), Unsafe.SizeOf<TTuple>());
        Span<T> TSpan = MemoryMarshal.Cast<byte, T>(byteSpan);
        span = new(TSpan, tuple.Length);
        return true;
    }

    internal static bool UnsafeTryConvertTupleSpan<TTuple, T>(this TTuple tuple, out TupleSpan<T> span)
		where TTuple : ITuple
		where T : unmanaged
    {
		Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<TTuple>());
        if (Unsafe.SizeOf<T>() != Unsafe.SizeOf<TTuple>() / tuple.Length)
        {
            span = default;
            return false;
        }
        if (Unsafe.SizeOf<TTuple>() % Unsafe.SizeOf<T>() != 0)
        {
            span = default;
            return false;
        }

        Span<byte> byteSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<TTuple, byte>(ref Unsafe.AsRef(tuple)), Unsafe.SizeOf<TTuple>());
        Span<T> TSpan = MemoryMarshal.Cast<byte, T>(byteSpan);
        span = new(TSpan, tuple.Length);
        return true;
    }
}

