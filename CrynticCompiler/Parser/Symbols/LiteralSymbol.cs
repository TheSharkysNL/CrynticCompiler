using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Symbols;

public abstract class LiteralSymbol<TData>(ISymbol<TData>? parent) : ILiteralSymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ISymbol<TData>? Parent => parent;
    
    public abstract TokenData<TData> Name { get; }
    public abstract object? Literal { get; }
    public abstract int Type { get; }
    
    public bool TryComputeExpression(out object? value)
    {
        value = Literal;
        return value is not null;
    }
}