using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace CrynticCompiler.Parser.Symbols;

public interface IExpressionSymbol<TData> : ISymbol<TData>  
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// tries to compute the expression
    /// </summary>
    /// <param name="value">the value from the expression</param>
    /// <returns>true if the expression could be computed else false</returns>
    public bool TryComputeExpression([MaybeNullWhen(false)] out object? value);
}