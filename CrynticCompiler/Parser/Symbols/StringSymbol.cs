using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Symbols;

public class StringSymbol<TData>(TokenData<TData> data, ISymbol<TData>? parent = null) : LiteralSymbol<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    private object? literal;

    public override TokenData<TData> Name => TokenData<TData>.Empty;

    public override object Literal
    {
        get
        {
            if (literal is not null)
            {
                return literal;
            }
            
            Encoding encoding = GetEncoding();
            ReadOnlySpan<TData> span = data.Span;
            ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(span);
            string encodedString = encoding.GetString(bytes);
            
            literal = encodedString;
            return literal;
        }
    }

    public static Encoding GetEncoding()
    {
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<byte>())
        {
            return Encoding.UTF8;
        }
        if (Unsafe.SizeOf<TData>() == Unsafe.SizeOf<char>())
        {
            if (BitConverter.IsLittleEndian)
            {
                return Encoding.Unicode;
            }

            return Encoding.BigEndianUnicode;
        }
        
        Debug.Assert(Unsafe.SizeOf<TData>() == Unsafe.SizeOf<int>());
        return new UTF32Encoding(!BitConverter.IsLittleEndian, false);
    }

    public override int Type => LiteralType.String;
}