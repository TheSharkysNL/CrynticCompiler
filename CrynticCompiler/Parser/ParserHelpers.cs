using System;
using System.Data;
using System.Numerics;
using System.Runtime.CompilerServices;
using CrynticCompiler.Tokenizer;
using CrynticCompiler.Extensions;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace CrynticCompiler.Parser;

public static class ParserHelpers
{
	public static bool NextTokenIsType<TData>(this ITokenEnumerator<TData> enumerator, int type)
		where TData : unmanaged, IBinaryInteger<TData> =>
		enumerator.MoveNext() & enumerator.Current.Type == type;

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2)
		where TData : unmanaged, IBinaryInteger<TData>
	{
		if (!enumerator.MoveNext())
			return false;

		int currType = enumerator.Current.Type;
		return IsType(currType, type, type2);
    }

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3)
		where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, type, type2, type3);
    }

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, type, type2, type3, type4);
    }

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, type, type2, type3, type4, type5);
    }


    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5, int type6)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, type, type2, type3, type4, type5, type6);
    }

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5, int type6, int type7)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, type, type2, type3, type4, type5, type6, type7);
    }

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5, int type6, int type7, int type8)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, type, type2, type3, type4, type5, type6, type7, type8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NextTokenIsTypes<TData, TTuple>(this ITokenEnumerator<TData> enumerator, TTuple tuple)
        where TData : unmanaged, IBinaryInteger<TData>
        where TTuple : ITuple
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, tuple);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, Span<int> span)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, span);
    }

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, params int[] types)
        where TData : unmanaged, IBinaryInteger<TData> =>
        enumerator.NextTokenIsTypes(types.AsSpan());

    public static bool NextTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, IEnumerable<int> enumerable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.MoveNext())
            return false;

        int currType = enumerator.Current.Type;
        return IsType(currType, enumerable);
    }

    public static bool NextPeekedTokenIsType<TData>(this ITokenEnumerator<TData> enumerator, int type, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData> =>
        enumerator.PeekNext(out consumable) & consumable.Token.Type == type;

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, type, type2);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, type, type2, type3);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, type, type2, type3, type4);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, type, type2, type3, type4, type5);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5, int type6, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, type, type2, type3, type4, type5, type6);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5, int type6, int type7, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, type, type2, type3, type4, type5, type6, type7);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, int type, int type2, int type3, int type4, int type5, int type6, int type7, int type8, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, type, type2, type3, type4, type5, type6, type7, type8);
    }

    public static bool NextPeekedTokenIsTypes<TData, TTuple>(this ITokenEnumerator<TData> enumerator, TTuple tuple, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
        where TTuple : ITuple
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, tuple);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, Span<int> span, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, span);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, IEnumerable<int> enumerable, out Consumable<TData> consumable)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, enumerable);
    }

    public static bool NextPeekedTokenIsTypes<TData>(this ITokenEnumerator<TData> enumerator, out Consumable<TData> consumable, params int[] ints)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (!enumerator.PeekNext(out consumable))
            return false;

        int currType = consumable.Token.Type;
        return IsType(currType, ints.AsSpan());
    }

    public static void MoveNextIfType<TData>(this ITokenEnumerator<TData> enumerator, int type)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (enumerator.Current.Type == type)
            enumerator.MoveNext();
    }

    public static bool IsType<T>(this Token<T> token, int type)
        where T : unmanaged, IBinaryInteger<T> =>
        token.Type == type;

    public static bool IsTypes<T>(this Token<T> token, int type, int type2)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, type, type2);

    public static bool IsTypes<T>(this Token<T> token, int type, int type2, int type3)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, type, type2, type3);

    public static bool IsTypes<T>(this Token<T> token, int type, int type2, int type3, int type4)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, type, type2, type3, type4);

    public static bool IsTypes<T>(this Token<T> token, int type, int type2, int type3, int type4, int type5)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, type, type2, type3, type4, type5);

    public static bool IsTypes<T>(this Token<T> token, int type, int type2, int type3, int type4, int type5, int type6)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, type, type2, type3, type4, type5, type6);

    public static bool IsTypes<T>(this Token<T> token, int type, int type2, int type3, int type4, int type5, int type6, int type7)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, type, type2, type3, type4, type5, type6, type7);

    public static bool IsTypes<T>(this Token<T> token, int type, int type2, int type3, int type4, int type5, int type6, int type7, int type8)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, type, type2, type3, type4, type5, type6, type7, type8);

    public static bool IsTypes<T, TTuple>(this Token<T> token, TTuple tuple)
        where T : unmanaged, IBinaryInteger<T>
        where TTuple : ITuple =>
        IsType(token.Type, tuple);

    public static bool IsTypes<T>(this Token<T> token, ReadOnlySpan<int> span)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, span);

    public static bool IsTypes<T>(this Token<T> token, params int[] @params)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, @params.AsSpan());

    public static bool IsTypes<T>(this Token<T> token, IEnumerable<int> enumerable)
        where T : unmanaged, IBinaryInteger<T> =>
        IsType(token.Type, enumerable);

    /// <summary>
    /// checks if token is any operator type:
    /// <see cref="TokenType.Plus"/>, <see cref="TokenType.Minus"/>,
    /// <see cref="TokenType.ForwardSlash"/>, <see cref="TokenType.Star"/>, <see cref="TokenType.Percentage"/>.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="token">the token to check for the operator type</param>
    /// <returns>true if the type is a operator else false</returns>
    public static bool IsOperatorType<TData>(this Token<TData> token)
        where TData : unmanaged, IBinaryInteger<TData> =>
        token.Type - TokenType.Plus <= TokenType.Percentage - TokenType.Plus;

    public const int LeftAssociation = 1;
    public const int RightAssociation = 0;

    public static (int Precedence, int Association) GetPrecedenceAndAssociation<TData>(this Token<TData> token)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        switch (token.Type - TokenType.Plus)
        {
            case 0:
                Debug.Assert(token.Type == TokenType.Plus);
                return (1, LeftAssociation);
            case 1:
                Debug.Assert(token.Type == TokenType.Minus);
                return (1, LeftAssociation);
            case 2:
                Debug.Assert(token.Type == TokenType.ForwardSlash);
                return (2, LeftAssociation);
            case 3:
                Debug.Assert(token.Type == TokenType.Star);
                return (2, LeftAssociation);
            case 4:
                Debug.Assert(token.Type == TokenType.Percentage);
                return (2, LeftAssociation);
            default:
                return default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, int type, int type2) =>
        currType == type | currType == type2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, int type, int type2, int type3) =>
        IsType(currType, type, type2) | currType == type3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, int type, int type2, int type3, int type4)
    {
        if (Vector128.IsHardwareAccelerated)
        {
            Vector128<int> vector = Vector128.Create(type, type2, type3, type4);

            Vector128<int> currTypeVector = Vector128.Create(currType);
            return Vector128.EqualsAny(currTypeVector, vector);
        }
        return IsType(currType, type, type2) | IsType(currType, type3, type4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, int type, int type2, int type3, int type4, int type5) =>
        IsType(currType, type, type2, type3, type4) || currType == type5;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, int type, int type2, int type3, int type4, int type5, int type6) =>
        IsType(currType, type, type2, type3, type4) || IsType(currType, type5, type6);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, int type, int type2, int type3, int type4, int type5, int type6, int type7) =>
        IsType(currType, type, type2, type3, type4) || IsType(currType, type5, type6, type7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, int type, int type2, int type3, int type4, int type5, int type6, int type7, int type8)
    {
        if (Vector256.IsHardwareAccelerated)
        {
            Vector256<int> vector = Vector256.Create(type, type2, type3, type4, type5, type6, type7, type8);

            Vector256<int> currTypeVector = Vector256.Create(currType);
            return Vector256.EqualsAny(currTypeVector, vector);
        }

        return IsType(currType, type, type2, type3, type4) || IsType(currType, type5, type6, type7, type8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, TupleSpan<int> span)
    {
        int spanLength = span.Length;
        for (int i = 0; i < spanLength; i++)
            if (span[i] == currType)
                return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType<TTuple>(int currType, TTuple tuple)
        where TTuple : ITuple
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TTuple>())
            if (tuple.UnsafeTryConvertTupleSpan(out TupleSpan<int> span))
                return IsType(currType, span);

        int tupleLength = tuple.Length;
        for (int i = 0; i < tupleLength; i++)
        {
            int val;
            try { val = (int)tuple[i]!; }
            catch { throw new ArgumentException("tuple must contain only ints"); }

            if (val == currType)
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, ReadOnlySpan<int> span)
    {
        int spanLength = span.Length;
        ref int reference = ref MemoryMarshal.GetReference(span);
        ref int endReference = ref Unsafe.Add(ref reference, spanLength);

        if (Vector256.IsHardwareAccelerated && spanLength >= Vector256<int>.Count)
        {
            Vector256<int> currTypeVec = Vector256.Create(currType);
            ref int vector256EndRef = ref Unsafe.Subtract(ref endReference, Vector256<int>.Count);
            do
            {
                Vector256<int> vector = Unsafe.As<int, Vector256<int>>(ref reference);
                if (Vector256.EqualsAny(currTypeVec, vector))
                    return true;
                reference = ref Unsafe.Add(ref reference, Vector256<int>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref reference, ref vector256EndRef));  // reference <= vector128EndRef

            nuint diff = UnsafeExtensions.GetReferenceDifference(ref reference, ref endReference);

            if (Vector128.IsHardwareAccelerated && diff >= (nuint)Vector128<int>.Count)
            {
                Vector128<int> currTypeVec128 = Vector128.Create(currType);
                Vector128<int> vector = Unsafe.As<int, Vector128<int>>(ref reference);
                if (Vector128.EqualsAny(currTypeVec128, vector))
                    return true;
                reference = ref Unsafe.Add(ref reference, Vector128<int>.Count);
            }
        }
        else if (Vector128.IsHardwareAccelerated && spanLength >= Vector128<int>.Count)
        {
            Vector128<int> currTypeVec = Vector128.Create(currType);
            ref int vector128EndRef = ref Unsafe.Subtract(ref endReference, Vector128<int>.Count);
            do
            {
                Vector128<int> vector = Unsafe.As<int, Vector128<int>>(ref reference);
                if (Vector128.EqualsAny(currTypeVec, vector))
                    return true;
                reference = ref Unsafe.Add(ref reference, Vector128<int>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref reference, ref vector128EndRef)); // reference <= vector128EndRef
        }

        while (Unsafe.IsAddressLessThan(ref reference, ref endReference))
        {
            if (reference == currType)
                return true;
            reference = ref Unsafe.Add(ref reference, 1);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsType(int currType, IEnumerable<int> enumerable)
    {
        foreach (int value in enumerable)
            if (value == currType)
                return true;
        return false;
    }
}

