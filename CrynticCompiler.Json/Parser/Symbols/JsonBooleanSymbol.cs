using System.Numerics;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Symbols;

public class JsonBooleanSymbol<TData>(bool truthy, ISymbol<TData>? parent = null) : LiteralSymbol<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public override TokenData<TData> Name => TokenData<TData>.Empty;

    private object? literal;
    public override object Literal => literal ??= truthy;
    
    public override int Type => JsonLiteralType.Bool;
}