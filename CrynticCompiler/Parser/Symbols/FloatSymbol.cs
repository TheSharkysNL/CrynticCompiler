using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Symbols;

public class FloatSymbol<TData>(TokenData<TData> data, ISymbol<TData>? parent = null) : LiteralSymbol<TData>(parent)
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

            if (TokenizerHelpers.TryParseFloatNumber(data.Span, out double value))
            {
                literal = value;
                return literal;
            }

            return null;
        }
    }
    
    public override int Type => LiteralType.Float;
}