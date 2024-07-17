namespace CrynticCompiler.Parser;

public readonly struct EmptyFormatter : ISpanFormattable
{
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        string.Empty;

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        charsWritten = 0;
        return true;
    }
}