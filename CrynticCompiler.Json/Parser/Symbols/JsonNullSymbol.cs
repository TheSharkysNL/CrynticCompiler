using System.Numerics;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Symbols;

public class JsonNullSymbol<TData>(ISymbol<TData>? parent = null) : ILiteralSymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ISymbol<TData>? Parent => parent;
    
    public TokenData<TData> Name => TokenData<TData>.Empty;
    
    public bool TryComputeExpression(out object? value)
    {
        value = null;
        return true;
    }

    public object? Literal => null;
    public int Type => JsonLiteralType.Null;
}