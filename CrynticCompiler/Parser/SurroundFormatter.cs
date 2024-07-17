using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrynticCompiler.Parser;

public readonly struct SurroundFormatter<T>(T value, string surroundCharacters) : ISpanFormattable
    where T : ISpanFormattable
{
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        string valueString = value.ToString(format, formatProvider);
        string str = new('\0', valueString.Length + surroundCharacters.Length * 2);

        ref char strRef = ref Unsafe.AsRef(in str.GetPinnableReference());
        Span<char> span = MemoryMarshal.CreateSpan(ref strRef, str.Length);
        
        surroundCharacters.CopyTo(span);
        valueString.CopyTo(span[surroundCharacters.Length..]);
        surroundCharacters.CopyTo(span[(surroundCharacters.Length + valueString.Length)..]);

        return str;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        Unsafe.SkipInit(out charsWritten);
        int surroundCharacterLength = surroundCharacters.Length;   
        if (!value.TryFormat(destination[surroundCharacterLength..], out int valueCharsWritten, format, provider) ||
            valueCharsWritten + surroundCharacterLength * 2 >= destination.Length)
        {
            return false;
        }
        
        surroundCharacters.CopyTo(destination);
        surroundCharacters.CopyTo(destination[(surroundCharacterLength + valueCharsWritten)..]);

        charsWritten = surroundCharacterLength * 2 + valueCharsWritten;
        return true;
    }
}