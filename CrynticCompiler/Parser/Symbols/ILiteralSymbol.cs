using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Symbols;

public interface ILiteralSymbol<TData> : IExpressionSymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public object? Literal { get; }
    
    /// <summary>
    /// see <see cref="LiteralType"/> for types
    /// </summary>
    public int Type { get; }
}