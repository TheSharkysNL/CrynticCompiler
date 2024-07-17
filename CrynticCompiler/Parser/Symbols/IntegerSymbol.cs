using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Symbols;

public class IntegerSymbol<TData>(TokenData<TData> data, int type, ISymbol<TData>? parent = null) : LiteralSymbol<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    private object? literal;
    
    public override TokenData<TData> Name => TokenData<TData>.Empty;

    public override object? Literal
    {
        get
        {
            if (literal is not null)
            {
                return literal;
            }

            int @base;
            if (type == TokenType.Number)
            {
                @base = 10;
            }
            else if (type == TokenType.Binary)
            {
                @base = 2;
            }
            else if (type == TokenType.Hexadecimal)
            {
                @base = 16;
            }
            else
            {
                Debug.Assert(type == TokenType.Octal);
                @base = 8;
            }

            if (TokenizerHelpers.TryParseNumber(data.Span, out long value, @base))
            {
                literal = value;
                return literal;
            }

            return null;
        }
    }

    public override int Type => LiteralType.Integer;
}