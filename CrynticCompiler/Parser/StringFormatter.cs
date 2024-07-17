namespace CrynticCompiler.Parser;

public readonly struct StringFormatter(string value) : ISpanFormattable
{
    public static implicit operator StringFormatter(string val) => new(val); 
    
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        value;

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < value.Length)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = value.Length;
        value.CopyTo(destination);
        return true;
    }
}