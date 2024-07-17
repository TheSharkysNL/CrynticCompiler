using System.Reflection;

namespace CrynticCompiler.Extensions;

// https://stackoverflow.com/a/22975649
public static class StreamReaderExtensions
{
    private static readonly Func<StreamReader, int> charPosField = ReflectionExtensions.CreateFieldGetterDelegate<StreamReader, int>("_charPos", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
    private static readonly Func<StreamReader, int> byteLenField = ReflectionExtensions.CreateFieldGetterDelegate<StreamReader, int>("_byteLen", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
    private static readonly Func<StreamReader, char[]> charBufferField = ReflectionExtensions.CreateFieldGetterDelegate<StreamReader, char[]>("_charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

    public static long GetPosition(this StreamReader reader)
    {
        // shift position back from BaseStream.Position by the number of bytes read
        // into internal byteLenField.
        int byteLen = byteLenField(reader);
        var position = reader.BaseStream.Position - byteLen;

        // if we have consumed chars from the buffer we need to calculate how many
        // bytes they represent in the current encoding and add that to the position.
        int charPos = charPosField(reader);
        if (charPos > 0)
        {
            var charBuffer = charBufferField(reader);
            var encoding = reader.CurrentEncoding;
            var bytesConsumed = encoding.GetByteCount(charBuffer, 0, charPos);
            position += bytesConsumed;
        }

        return position;
    }

    public static void SetPosition(this StreamReader reader, long position)
    {
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(position, SeekOrigin.Begin);
    }
}