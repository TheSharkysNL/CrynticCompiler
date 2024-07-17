using System;
using System.Numerics;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Parser.Nodes;
namespace CrynticCompiler.Parser;

public interface ISemanticModel<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    /// <summary>
    /// gets the <see cref="ISymbol{TData}"/> for the given <paramref name="node"/>
    /// </summary>
    /// <param name="node">the node to get the <see cref="ISymbol{TData}"/> for</param>
    /// <returns>the <see cref="ISymbol{TData}"/> for the given node</returns>
    public ISymbol<TData>? GetSymbol(INode? node);
}

