using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using CrynticCompiler.Extensions;
using CrynticCompiler.Tokenizer.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrynticCompiler.Tokenizer;

public static class TokenizerHelpers
{
    /// <summary>
    /// checks if a <paramref name="value"/> is less than or equal <paramref name="b"/> but greater than 0
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="value"></param>
    /// <param name="b"></param>
    /// <returns>true if value is less than or equal to <paramref name="b"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBelowOrEqual<TData>(TData value, TData b)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<byte>())
            return Unsafe.As<TData, byte>(ref value) <= Unsafe.As<TData, byte>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<ushort>())
            return Unsafe.As<TData, ushort>(ref value) <= Unsafe.As<TData, ushort>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<uint>())
            return Unsafe.As<TData, uint>(ref value) <= Unsafe.As<TData, uint>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<ulong>())
            return Unsafe.As<TData, ulong>(ref value) <= Unsafe.As<TData, ulong>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<UInt128>())
            return Unsafe.As<TData, UInt128>(ref value) <= Unsafe.As<TData, UInt128>(ref b);
        if (IsSigned<TData>())
            return TData.IsPositive(value) & value <= b;
        return value <= b;
    }

    /// <summary>
    /// checks if a <paramref name="value"/> is less than <paramref name="b"/> but greater than 0
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="value"></param>
    /// <param name="b"></param>
    /// <returns>true if value is less than to <paramref name="b"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBelow<TData>(TData value, TData b)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<byte>())
            return Unsafe.As<TData, byte>(ref value) < Unsafe.As<TData, byte>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<ushort>())
            return Unsafe.As<TData, ushort>(ref value) < Unsafe.As<TData, ushort>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<uint>())
            return Unsafe.As<TData, uint>(ref value) < Unsafe.As<TData, uint>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<ulong>())
            return Unsafe.As<TData, ulong>(ref value) < Unsafe.As<TData, ulong>(ref b);
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<UInt128>())
            return Unsafe.As<TData, UInt128>(ref value) < Unsafe.As<TData, UInt128>(ref b);
        if (IsSigned<TData>())
            return TData.IsPositive(value) & value < b;
        return value < b;
    }

    /// <summary>
    /// checks if a <see cref="IBinaryInteger{TSelf}"/> is signed or not
    /// </summary>
    /// <typeparam name="T">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <returns>true if the integer is signed else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigned<T>() where T : IBinaryInteger<T> =>
        T.IsNegative(T.Zero - T.One);

    /// <summary>
    /// checks if the <typeparamref name="TData"/> is a new line, eg: '\n'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a new line else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNewLine<TData>(TData value) where TData : IBinaryInteger<TData> =>
        value == TData.CreateTruncating('\n');

    /// <summary>
    /// checks if the <typeparamref name="TData"/> is a whitespace character, eg: ' \n\t\r'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a whitespace character else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhitespace<TData>(TData value) where TData : IBinaryInteger<TData> =>
        IsSpace(value) | IsNewLine(value)
        | value == TData.CreateTruncating('\r');

    /// <summary>
    /// checks if the <typeparamref name="TData"/> is a space character, eg: ' \t'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a space character else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSpace<TData>(TData value) where TData : IBinaryInteger<TData> =>
        value == TData.CreateTruncating(' ') | value == TData.CreateTruncating('\t');

    /// <summary>
    /// <inheritdoc cref="GetWhileTrue{TData}"/>
    /// whilst also including new lines
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="initialValue">the initial value for from the <paramref name="enumerator"/></param>
    /// <param name="enumerator">the enumerator to read from</param>
    /// <param name="predicate">the values to include within the returned value</param>
    /// <param name="line">the line that the function ends on, starts at 0 no matter the <paramref name="index"/></param>
    /// <param name="column">the column that the function ends on, starts at 0 no matter the <paramref name="index"/></param>
    /// <returns>
    ///          a TData[] that contains the values and new lines in a row
    ///          that were true from the <paramref name="predicate"/>
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">if the <paramref name="index"/> is greater than or equal to the length of the <paramref name="data"/></exception>
    

    /// <summary>
    /// checks if a <paramref name="value"/> is between <paramref name="min"/> and <paramref name="max"/>
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check if it is between <paramref name="min"/> and <paramref name="max"/></param>
    /// <param name="min">the minimum that the <paramref name="value"/> can be</param>
    /// <param name="max">the maximum that the <paramref name="value"/> can be</param>
    /// <returns>true if the <paramref name="value"/> is between <paramref name="min"/> and <paramref name="max"/> else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool IsBetween<TData>(TData value, TData min, TData max)
        where TData : unmanaged, IBinaryInteger<TData> =>
        IsBelowOrEqual(value - min, max - min);

    /// <summary>
    /// checks if a <paramref name="value"/> is a number character, eg: '0123456789'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a number character else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigit<TData>(TData value) where TData : unmanaged, IBinaryInteger<TData> =>
        IsBetween(value, TData.CreateTruncating('0'), TData.CreateTruncating('9'));

    /// <summary>
    /// checks if a <paramref name="value"/> is a lowercase letter, from 'a' to 'z'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a lowercase letter else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLowercase<TData>(TData value) where TData : unmanaged, IBinaryInteger<TData> =>
        IsBetween(value, TData.CreateTruncating('a'), TData.CreateTruncating('z'));

    /// <summary>
    /// checks if a <paramref name="value"/> is a uppercase letter, from 'A' to 'Z'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a uppercase letter else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUppercase<TData>(TData value) where TData : unmanaged, IBinaryInteger<TData> =>
        IsBetween(value, TData.CreateTruncating('A'), TData.CreateTruncating('Z'));

    /// <summary>
    /// checks if a <paramref name="value"/> is a letter, from 'a' to 'z' and 'A' to 'Z'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a letter else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetter<TData>(TData value)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        TData masked = (value & TData.CreateTruncating(-33));
        return IsBetween(masked, TData.CreateTruncating('A'), TData.CreateTruncating('Z'));
    }

    /// <summary>
    /// checks if a <paramref name="value"/> is a letter, from 'a' to 'z' and 'A' to 'Z'
    /// or a digit, eg: '0123456789'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a letter or a digit else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlphanumeric<TData>(TData value) where TData : unmanaged, IBinaryInteger<TData> =>
        IsLetter(value) | IsDigit(value);

    /// <summary>
    /// checks if a <paramref name="value"/> is equal to the character <paramref name="c"/>
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <param name="c">the character that will be used to check against the <paramref name="value"/></param>
    /// <returns>true if the <paramref name="value"/> is equal to <paramref name="c"/> else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCharacter<TData>(TData value, char c) where TData : IBinaryInteger<TData> =>
        value == TData.CreateTruncating(c);

    /// <summary>
    /// checks if a <paramref name="value"/> is a hexadecimal character,
    /// eg: '0123456789abcdef' or '0123456789ABCDEF'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the <paramref name="value"/> is a hexadecimal character else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHexadecimal<TData>(TData value)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        return IsBetween(value & TData.CreateTruncating(-33), TData.CreateTruncating('A'), TData.CreateTruncating('Z')) |
            IsDigit(value);
    }

    /// <summary>
    /// checks a <see cref="Reader{TData}"/> if the next few characters are equal to the <paramref name="values"/>
    /// if not the reader will be put back to its starting position
    /// </summary>
    /// <param name="reader">the reader to read from</param>
    /// <param name="values">the values to check for a match</param>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <returns><see langword="true"/> if all <paramref name="values"/> are equal to the next characters from the <paramref name="reader"/> else <see langword="false"/></returns>
    public static bool AreNextCharacters<TData>(Reader<TData> reader, ReadOnlySpan<char> values)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        long startPosition = reader.Position;

        for (int i = 0; i < values.Length; i++)
        {
            char value = values[i];

            TData check = reader.Read(out bool eof);
            if (!eof & IsCharacter(check, value)) continue;
            
            reader.Position = startPosition;
            return false;
        }

        return true;
    }

    /// <summary>
    /// checks a <see cref="Reader{TData}"/> if the next few characters are equal to the <paramref name="values"/>
    /// if not the reader will be put back to its starting position
    /// </summary>
    /// <param name="reader">the reader to read from</param>
    /// <param name="values">the values to check for a match</param>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <returns><see langword="true"/> if all <paramref name="values"/> are equal to the next characters from the <paramref name="reader"/> else <see langword="false"/></returns>
    public static bool AreNextCharacters<TData>(Reader<TData> reader, ReadOnlyMemory<TData> values)
        where TData : unmanaged, IBinaryInteger<TData> =>
        AreNextCharacters(reader, values.Span);
    
    /// <summary>
    /// checks a <see cref="Reader{TData}"/> if the next few characters are equal to the <paramref name="values"/>
    /// if not the reader will be put back to its starting position
    /// </summary>
    /// <param name="reader">the reader to read from</param>
    /// <param name="values">the values to check for a match</param>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <returns><see langword="true"/> if all <paramref name="values"/> are equal to the next characters from the <paramref name="reader"/> else <see langword="false"/></returns>
    public static bool AreNextCharacters<TData>(Reader<TData> reader, ReadOnlySpan<TData> values)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        long startPosition = reader.Position;

        for (int i = 0; i < values.Length; i++)
        {
            TData value = values[i];

            TData check = reader.Read(out bool eof);
            if (!eof & check == value) continue;
            
            reader.Position = startPosition;
            return false;
        }

        return true;
    }

    //const int TabSpaceAmount = 4;

    //public static int GetIndentLevel<TData>(ReadOnlyMemory<TData> data, int index) where TData : unmanaged, IBinaryInteger<TData>
    //{
    //    if (index >= data.Length)
    //        throw new ArgumentOutOfRangeException(nameof(index));

    //    ReadOnlySpan<TData> span = data.Span;
    //    if (!IsNewLine(span[index]))
    //        throw new ArgumentException("to calculate the indentation level the value at the index must be a new line");

    //    int indentLevel = 0;
    //    for (int i = index + 1; i < data.Length; i++)
    //    {
    //        TData val = span[i];
    //        if (!IsSpace(val))
    //            break;

    //        bool isTab = val == TData.CreateTruncating('\t');
    //        indentLevel += 1 + (Unsafe.As<bool, byte>(ref isTab) * (TabSpaceAmount - 1));
    //    }

    //    return indentLevel;
    //}

    /// <summary>
    /// checks if the <paramref name="value"/> is a '#'
    /// </summary>
    /// <typeparam name="TData">the <see cref="IBinaryInteger{TSelf}"/> type</typeparam>
    /// <param name="value">the value to check</param>
    /// <returns>true if the value is a '#' else false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHashtag<TData>(TData value) where TData : IBinaryInteger<TData> =>
        value == TData.CreateTruncating('#');

    /// <summary>
    /// gets the max value of the given <typeparamref name="TData"/>,
    /// eg: <see cref="int.MaxValue"/> for <see cref="int"/>
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <returns>the maximum value that <typeparamref name="TData"/> can store</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TData GetMaxValue<TData>()
        where TData : IBinaryInteger<TData>
    {
        if (IsSigned<TData>())
        {
            TData mostSiginficantBit = TData.One << (Unsafe.SizeOf<TData>() * 8 - 1);
            return ~mostSiginficantBit;
        }
        return ~TData.Zero;
    }


    /// <summary>
    /// gets the min value of the given <typeparamref name="TData"/>,
    /// eg: <see cref="int.MinValue"/> for <see cref="int"/>
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <returns>the minimum value that <typeparamref name="TData"/> can store</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TData GetMinValue<TData>()
        where TData : IBinaryInteger<TData>
    {
        if (IsSigned<TData>())
            return TData.One << (Unsafe.SizeOf<TData>() * 8 - 1);
        return TData.Zero;
    }

    #region ParseInt

    private static int GetCharacterCount<TData>(int @base)
        where TData : IBinaryInteger<TData> =>
        (int)(double.CreateTruncating(TData.Log2(GetMaxValue<TData>())) / Math.Log2(@base)) + 1; // for any case

    private static bool WillOverflow<TData, TOut>(TData currNumber, TOut number, int @base, [NotNullWhen(false)] out TOut? newNumber)
        where TData : unmanaged, IBinaryInteger<TData>
        where TOut : IBinaryInteger<TOut>
    {
        TOut mulBase = number * TOut.CreateTruncating(@base);
        if (mulBase / TOut.CreateTruncating(@base) != number)
        {
            newNumber = default;
            return true;
        }
        number = mulBase;
        TOut addCurrNumber = number + TOut.CreateTruncating(currNumber);
        if (addCurrNumber < number)
        {
            newNumber = default;
            return true;
        }
        newNumber = addCurrNumber;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseIntBelow11<TData, TOut>(ReadOnlySpan<TData> span, int @base, [NotNullWhen(true)] out TOut? value)
        where TData : unmanaged, IBinaryInteger<TData>
        where TOut : IBinaryInteger<TOut>
    {
        int length = span.Length;
        Debug.Assert(length != 0);
        TData first = span[0];
        int start = 1;

        bool isNegative = false;
        if (IsCharacter(first, '-'))
        {
            if (!IsSigned<TOut>())
            {
                value = default;
                return false;
            }
            if (length == 1)
            {
                value = default;
                return false;
            }
            isNegative = true;
            first = span[1];
            start = 2;
        }

        bool checkOverflow = false;
        int maxCharacterCount = GetCharacterCount<TOut>(@base);
        if (length - start + 1 >= maxCharacterCount)
        {
            if (length - start + 1 == maxCharacterCount)
                checkOverflow = true;
            else
            {
                value = default;
                return false;
            }
        }

        if (!IsDigit(first))
        {
            value = default;
            return false;
        }
        TData firstNumber = first - TData.CreateTruncating('0');

        TOut number = TOut.CreateTruncating(firstNumber);
        for (int i = start; i < length - Unsafe.As<bool, byte>(ref checkOverflow); i++)
        {
            TData currNumber = span[i] - TData.CreateTruncating('0');
            if (!IsBelow(currNumber, TData.CreateTruncating(@base)))
            {
                value = default;
                return false;
            }

            number *= TOut.CreateTruncating(@base);
            number += TOut.CreateTruncating(currNumber);
        }

        if (checkOverflow)
        {
            TData currNumber = span[length - 1] - TData.CreateTruncating('0');
            if (!IsBelow(currNumber, TData.CreateTruncating(@base)))
            {
                value = default;
                return false;
            }
            if (WillOverflow(currNumber, number, @base, out TOut? newNumber))
            {
                value = default;
                return false;
            }
            number = newNumber;
        }

        value = isNegative ? -number : number;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseIntAbove10<TData, TOut>(ReadOnlySpan<TData> span, int @base, [NotNullWhen(true)] out TOut? value)
        where TData : unmanaged, IBinaryInteger<TData>
        where TOut : IBinaryInteger<TOut>
    {
        int length = span.Length;
        Debug.Assert(length != 0);
        TData first = span[0];
        int start = 1;

        bool isNegative = false;
        if (IsCharacter(first, '-'))
        {
            if (!IsSigned<TOut>())
            {
                value = default;
                return false;
            }
            if (length == 1)
            {
                value = default;
                return false;
            }
            isNegative = true;
            first = span[1];
            start = 2;
        }

        bool checkOverflow = false;
        int maxCharacterCount = GetCharacterCount<TOut>(@base);
        if (length - start + 1 >= maxCharacterCount)
        {
            if (length - start + 1 == maxCharacterCount)
                checkOverflow = true;
            else
            {
                value = default;
                return false;
            }
        }

        TData minCharacter = TData.CreateTruncating(':');
        TData maxCharacter = TData.CreateTruncating('@' + (@base - 10));
        TData ten = TData.CreateTruncating(10);

        TData firstAsLower = first | TData.CreateTruncating(0x20);
        bool firstIsCharacter = IsLowercase(firstAsLower);

        // 0x20 is to convert alpha characters to lowercase and digits will stay the same 
        // then subtract 0x30 which is the character '0' or 0x57 which is the character 'a' - 10 if it is a character
        TData firstNumber = firstAsLower - TData.CreateTruncating('0' + 0x27 * Unsafe.As<bool, byte>(ref firstIsCharacter));
        if (!IsBelow(firstNumber, ten) | IsBetween(firstNumber, minCharacter, maxCharacter))
        {
            value = default;
            return false;
        }

        TOut number = TOut.CreateTruncating(firstNumber);
        for (int i = start; i < length - Unsafe.As<bool, byte>(ref checkOverflow); i++)
        {
            TData current = span[i];
            TData currentAsLower = current | TData.CreateTruncating(0x20);
            bool currentIsCharacter = IsLowercase(currentAsLower);

            // same as above
            TData converted = currentAsLower - TData.CreateTruncating('0' + 0x27 * Unsafe.As<bool, byte>(ref currentIsCharacter));
            if (!IsBelow(firstNumber, ten) | IsBetween(firstNumber, minCharacter, maxCharacter))
            {
                value = default;
                return false;
            }

            number *= TOut.CreateTruncating(@base);
            number += TOut.CreateTruncating(converted);
        }

        if (checkOverflow)
        {
            TData currNumber = span[^1];
            TData currentAsLower = currNumber | TData.CreateTruncating(0x20);
            bool currentIsCharacter = IsLowercase(currentAsLower);

            // same as above
            TData converted = currentAsLower - TData.CreateTruncating('0' + 0x27 * Unsafe.As<bool, byte>(ref currentIsCharacter));
            if (!IsBelow(firstNumber, ten) | IsBetween(firstNumber, minCharacter, maxCharacter))
            {
                value = default;
                return false;
            }
            if (WillOverflow(converted, number, @base, out TOut? newNumber))
            {
                value = default;
                return false;
            }
            number = newNumber;
        }

        value = isNegative ? -number : number;
        return true;
    }

    private static bool ValidatePrefix<TData>(ReadOnlySpan<TData> span, int @base, out int start)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        if (span.Length > 1 && IsLowercase(span[1]))
        {
            char c = @base switch
            {
                2 => 'b',
                8 => 'o',
                16 => 'x',
                _ => '\0'
            };

            start = 2;
            return c != '\0' && IsCharacter(span[0], '0') && IsCharacter(span[1], c);
        }

        start = 0;
        return true;
    }
    
    /// <summary>
    /// tries to parse a binary number using the data from <paramref name="span"/>
    /// and a given <paramref name="base"/>. any base from 2 to 36 is allowed.
    /// the <paramref name="span"/> can contain number prefixes like '-', '0b' or '0x', etc.
    /// </summary>
    /// <typeparam name="TData">the type of data in the <paramref name="span"/></typeparam>
    /// <typeparam name="TOut">the type of <see cref="IBinaryInteger{TSelf}"/> that will be returned</typeparam>
    /// <param name="span">the span containing the number data</param>
    /// <param name="base">the base of the number, anywhere from 2 to 36</param>
    /// <param name="value">the number that has been parsed</param>
    /// <returns>true if the number could be parsed else false</returns>
    public static bool TryParseNumber<TData, TOut>(ReadOnlySpan<TData> span, [NotNullWhen(true)] out TOut? value, int @base = 10)
        where TData : unmanaged, IBinaryInteger<TData>
        where TOut : IBinaryInteger<TOut>
    {
        if ((uint)@base > 36 | @base < 2) // 36 is 0-9 and a-z
        {
            value = default;
            return false;
        }

        int length = span.Length;
        if (length == 0)
        {
            value = TOut.Zero;
            return true;
        }

        if (!ValidatePrefix(span, @base, out int start))
        {
            value = TOut.Zero;
            return false;
        }

        span = span[start..];
        if (@base <= 10)
            return TryParseIntBelow11(span, @base, out value);
        return TryParseIntAbove10(span, @base, out value);
    }

    /// <summary>
    /// parses a binary number using the data from <paramref name="span"/>
    /// and a given <paramref name="base"/>. any base from 2 to 36 is allowed.
    /// the <paramref name="span"/> should not contain any number prefixes except '-'
    /// (not '0x' or '0b')
    /// </summary>
    /// <typeparam name="TData">the type of data in the <paramref name="span"/></typeparam>
    /// <typeparam name="TOut">the type of <see cref="IBinaryInteger{TSelf}"/> that will be returned</typeparam>
    /// <param name="span">the span containing the number data</param>
    /// <param name="base">the base of the number, anywhere from 2 to 36</param>
    /// <returns>the number that has been parsed</returns>
    /// <exception cref="ArgumentException"></exception>
    public static TOut ParseNumber<TData, TOut>(ReadOnlySpan<TData> span, int @base = 10)
        where TData : unmanaged, IBinaryInteger<TData>
        where TOut : IBinaryInteger<TOut>
    {
        if ((uint)@base > 36 | @base < 2) // 36 is 0-9 and a-z
            throw new ArgumentException("base cannot be greater than 36 or less than 2", nameof(@base));

        if (!ValidatePrefix(span, @base, out _))
            throw new ArgumentException($"invalid prefix for base: {@base} number, or number contains letters",
                nameof(span));
                
        if (TryParseNumber(span, out TOut? value, @base))
            return value;
        throw new ArgumentException($"the given {nameof(span)} could not be parsed");
    }

    /// <summary>
    /// parses a floating point number using the data from <paramref name="span"/>.
    /// the parser can only use base 10.
    /// </summary>
    /// <typeparam name="TData">the type of data in the <paramref name="span"/></typeparam>
    /// <typeparam name="TOut">the type of <see cref="IFloatingPoint{TSelf}"/> that will be returned</typeparam>
    /// <param name="span">the span containing the number data</param>
    /// <param name="value">the number that has been parsed</param>
    /// <param name="decimalSeperator">the seperator between the upper and lower half of a decimal number</param>
    /// <returns>true if the number could be parsed else false</returns>
    public static bool TryParseFloatNumber<TData, TOut>(ReadOnlySpan<TData> span, [NotNullWhen(true)] out TOut? value, char decimalSeperator = '.')
        where TData : IBinaryInteger<TData>
        where TOut : IFloatingPoint<TOut>
    {
        int length = span.Length;
        if (length == 0)
        {
            value = TOut.Zero;
            return true;
        }

        TData first = span[0];
        int start = 1;

        bool isNegative = false;
        if (IsCharacter(first, '-'))
        {
            if (length == 1)
            {
                value = default;
                return false;
            }
            isNegative = true;
            first = span[1];
            start = 2;
        }

        int divisor = 0;
        if (IsCharacter(first, decimalSeperator))
        {
            int newStart = start + 1;
            if (length == newStart)
            {
                value = default;
                return false;
            }

            divisor = 10;
            first = span[start];
            start = newStart;
        }

        TData firstNumber = first - TData.CreateTruncating('0');
        if (TData.IsNegative(firstNumber) | firstNumber >= TData.CreateTruncating(10))
        {
            value = default;
            return false;
        }

        TOut number = TOut.CreateTruncating(firstNumber);
        if (divisor != 0)
            number /= TOut.CreateTruncating(divisor);

        for (int i = start; i < length; i++)
        {
            TData curr = span[i];
            if (IsCharacter(curr, decimalSeperator))
            {
                divisor = 1;
                continue;
            }
            TData currNumber = curr - TData.CreateTruncating('0');
            if (TData.IsNegative(currNumber) | currNumber >= TData.CreateTruncating(10))
            {
                value = default;
                return false;
            }

            divisor *= 10;
            if (divisor == 0)
            {
                number *= TOut.CreateTruncating(10);
                number += TOut.CreateTruncating(currNumber);
            }
            else
            {
                number += TOut.CreateTruncating(currNumber) / TOut.CreateTruncating(divisor);
            }

        }

        value = isNegative ? -number : number;
        return true;
    }

    /// <summary>
    /// parses a floating point number using the data from <paramref name="span"/>.
    /// the parser can only use base 10.
    /// </summary>
    /// <typeparam name="TData">the type of data in the <paramref name="span"/></typeparam>
    /// <typeparam name="TOut">the type of <see cref="IFloatingPoint{TSelf}"/> that will be returned</typeparam>
    /// <param name="span">the span containing the number data</param>
    /// <param name="decimalSeperator">the seperator between the upper and lower half of a decimal number</param>
    /// <returns>the number that has been parsed</returns>
    /// <exception cref="ArgumentException"></exception>
    public static TOut ParseFloatNumber<TData, TOut>(ReadOnlySpan<TData> span, char decimalSeperator = '.')
        where TData : IBinaryInteger<TData>
        where TOut : IFloatingPoint<TOut>
    {
        if (TryParseFloatNumber(span, out TOut? value, decimalSeperator))
            return value;
        throw new ArgumentException($"the given {nameof(span)} could not be parsed");
    }
    #endregion

    private static class Numbers<TData>
        where TData : unmanaged, IBinaryInteger<TData>
    {
        public static readonly TData[] values =
            ("00010203040506070809"u8 +
            "10111213141516171819"u8 +
            "20212223242526272829"u8 +
            "30313233343536373839"u8 +
            "40414243444546474849"u8 +
            "50515253545556575859"u8 +
            "60616263646566676869"u8 +
            "70717273747576777879"u8 +
            "80818283848586878889"u8 +
            "90919293949596979899"u8).Convert<byte, TData>();

        public static readonly TData[] zero = { TData.CreateTruncating('0') };
    }

    public static TData[] IntToArray<TData, TNumber>(TNumber value)
        where TData : unmanaged, IBinaryInteger<TData>
        where TNumber : IBinaryInteger<TNumber>
    {
        if (value is 0)
            return Numbers<TData>.zero;
        int digits = (int)(Math.Log2(double.CreateTruncating(TNumber.Abs(value))) / Math.Log2(10)) + 1;

        bool isNegative = TNumber.IsNegative(value);
        TData[] values = new TData[digits + Unsafe.As<bool, byte>(ref isNegative)];

        values[0] = TData.CreateTruncating('-');

        ref TData reference = ref MemoryMarshal.GetArrayDataReference(values);
        ref TData endReference = ref Unsafe.Add(ref reference, values.Length);

        ref TData numbersReference = ref MemoryMarshal.GetArrayDataReference(Numbers<TData>.values);

        if (value >= TNumber.CreateTruncating(10))
        {
            while (value >= TNumber.CreateTruncating(100))
            {
                endReference = ref Unsafe.Subtract(ref endReference, 2);
                (value, TNumber remainder) = TNumber.DivRem(value, TNumber.CreateTruncating(100));
                Unsafe.CopyBlockUnaligned(
                    ref Unsafe.As<TData, byte>(ref endReference),
                    ref Unsafe.As<TData, byte>(ref Unsafe.Add(ref numbersReference, Unsafe.SizeOf<TData>() * 2 * int.CreateTruncating(remainder))),
                    (uint)Unsafe.SizeOf<TData>() * 2);
            }

            if (value >= TNumber.CreateTruncating(10))
            {
                endReference = ref Unsafe.Subtract(ref endReference, 2);
                (value, TNumber remainder) = TNumber.DivRem(value, TNumber.CreateTruncating(100));
                Unsafe.CopyBlockUnaligned(
                    ref Unsafe.As<TData, byte>(ref endReference),
                    ref Unsafe.As<TData, byte>(ref Unsafe.Add(ref numbersReference, Unsafe.SizeOf<TData>() * 2 * int.CreateTruncating(remainder))),
                    (uint)Unsafe.SizeOf<TData>() * 2);
                return values;
            }
        }

        
        Unsafe.Subtract(ref endReference, 1) = TData.CreateTruncating(value + TNumber.CreateTruncating('0'));
        return values;
    }

    // internal static void ConnectTokens<TData>(List<Token<TData>> tokens, IReadOnlyList<Token<TData>> current, IReadOnlyList<Token<TData>> next)
    //     where TData : unmanaged, IBinaryInteger<TData>
    // {
    //     int currentCount = current.Count;
    //     int nextCount = next.Count;
    //
    //     if (nextCount == 0)
    //         tokens.AddRange(current);
    //     if (currentCount == 0)
    //         tokens.AddRange(next);
    //     tokens.Capacity += current.Count + next.Count;
    //
    //     Token<TData> lastToken = current[^1];
    //     Token<TData> firstToken = next[0];
    //     if (lastToken.Type != TokenType.Identifier | firstToken.Type != TokenType.Identifier)
    //     {
    //         tokens.AddRange(current);
    //         tokens.AddRange(next);
    //     }
    //
    //     int currentCountMin1 = current.Count - 1;
    //     for (int i = 0; i < currentCountMin1; i++)
    //         tokens.Add(current[i]);
    //
    //     Token<TData> connectedToken = new(lastToken.Data.Concat(firstToken.Data), TokenType.Identifier);
    //     tokens.Add(connectedToken);
    //
    //     for (int i = 1; i < nextCount; i++)
    //         tokens.Add(next[i]);
    // }

    public static bool TryGetTokenTypeName(int tokenType, [NotNullWhen(true)] out string? name)
    {
        Type tokenTypeClass = typeof(TokenType);

        FieldInfo[] fields = tokenTypeClass.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (FieldInfo field in fields)
        {
            if (tokenType == (int)field.GetValue(null)!)
            {
                name = $"{nameof(TokenType)}.{field.Name}";
                return true;
            }
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        ValueListBuilder<char> builder = new(stackalloc char[512]);
        foreach (Assembly assembly in assemblies)
        {
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                //if (!type.IsClass || ReferenceEquals(type, tokenTypeClass) || !tokenTypeClass.IsAssignableFrom(type))
                //	continue;
                if (!ReferenceEquals(type.BaseType, tokenTypeClass))
                    continue;

                FieldInfo[] typeFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

                bool foundMember = false; // a type can only hold 1 member with the types value, if found we can break
                foreach (FieldInfo field in typeFields)
                {
                    if (tokenType.Equals(field.GetValue(null)))
                    {
                        builder = AddTypeAndMember(builder, type, field);
                        foundMember = true;
                        break;
                    }
                }

                if (foundMember)
                    break;

                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

                foreach (PropertyInfo property in properties)
                {
                    if (tokenType.Equals(property.GetValue(null)))
                    {
                        builder = AddTypeAndMember(builder, type, property);
                        foundMember = true;
                        break;
                    }
                }

                if (foundMember)
                    break;
            }
        }

        if (builder.Length == 0)
        {
            name = string.Empty;
            return false; // this should only be able to fail if a type was used that doesn't implement TokenType
        }

        ReadOnlySpan<char> chars = builder.AsSpan();
        name = new(chars);
        builder.Dispose();
        return true;
    }

    private static ValueListBuilder<char> AddTypeAndMember(ValueListBuilder<char> builder, Type type, MemberInfo member)
    {
        if (builder.Length != 0)
            builder.Append(" | ");

        builder.Append(type.Name);
        builder.Append('.');
        builder.Append(member.Name);

        return builder;
    }
}
