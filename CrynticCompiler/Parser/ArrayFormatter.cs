using System.Runtime.CompilerServices;

namespace CrynticCompiler.Parser;

public readonly struct ArrayFormatter<T>(T[] array, string seperator, string surroundCharacter = "") : ISpanFormattable
    where T : ISpanFormattable
{
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        ValueListBuilder<char> builder = new(stackalloc char[512]);

        if (array.Length != 0)
        {
            AppendValue(ref builder, array[0], format, formatProvider);

            for (int i = 1; i < array.Length; i++)
            {
                builder.Append(seperator);
                AppendValue(ref builder, array[i], format, formatProvider);
            }
        }

        ReadOnlySpan<char> span = builder.AsSpan();
        builder.Dispose();
        return span.ToString();
    }

    private void AppendValue(ref ValueListBuilder<char> builder, T value, string? format, IFormatProvider? formatProvider)
    {
        string valueStr = value.ToString(format, formatProvider);
        builder.Append(surroundCharacter);
        builder.Append(valueStr);
        builder.Append(surroundCharacter);
    }

    private int Format(Span<char> destination, T value, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (!value.TryFormat(destination[surroundCharacter.Length..], out int charsWritten, format, provider))
        {
            return -1;
        }
        
        surroundCharacter.CopyTo(destination);
        surroundCharacter.CopyTo(destination[(surroundCharacter.Length + charsWritten)..]);

        return surroundCharacter.Length * 2 + charsWritten;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        Unsafe.SkipInit(out charsWritten);
        if (array.Length != 0)
        {
            int position = Format(destination, array[0], format, provider);
            if (position == -1)
            {
                return false;
            }

            charsWritten = position;
            destination = destination[position..];

            for (int i = 1; i < array.Length; i++)
            {
                if (!seperator.TryCopyTo(destination))
                {
                    return false;
                }

                int newPosition = Format(destination[seperator.Length..], array[i], format, provider) + seperator.Length;
                charsWritten += newPosition;
                if (newPosition == -1)
                {
                    return false;
                }
                destination = destination[newPosition..];
            }
        }
        
        return true;
    }
}