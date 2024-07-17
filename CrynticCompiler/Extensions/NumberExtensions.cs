using System.Numerics;
using System.Runtime.CompilerServices;

namespace CrynticCompiler.Extensions;

public static class NumberExtensions
{
    /// <summary>
    /// gets the bit at a given <paramref name="index"/>
    /// </summary>
    /// <typeparam name="T">the number type</typeparam>
    /// <param name="b">the number in which to get the bit</param>
    /// <param name="index">the index of the bit</param>
    /// <returns>the bit at the <paramref name="index"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetBit<T>(this T b, int index) where T : IBinaryInteger<T> =>
        b & (T.CreateTruncating(1) << index);

    /// <summary>
    /// sets a bit at a given <paramref name="index"/>
    /// </summary>
    /// <typeparam name="T">the number type</typeparam>
    /// <param name="b">the number in which to set the bit</param>
    /// <param name="value">true to set the bit else false</param>
    /// <param name="index">the index of the bit</param>
    /// <returns>the same number with the bit change according to the <paramref name="value"/> at the <paramref name="index"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SetBit<T>(this T b, bool value, int index)
        where T : IBinaryInteger<T>
    {
        byte valueByte = Unsafe.As<bool, byte>(ref value);
        T valueAtIndex = T.CreateTruncating(valueByte) << index;
        return (b & ~(T.One << index)) | valueAtIndex;
    }
}
