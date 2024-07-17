using System;
using System.Numerics;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Parser.Nodes;
namespace CrynticCompiler.Parser;

public interface ISemanticModel<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{ // TODO: add descriptions for the functions within this interface
    public ISymbol<TData>? GetSymbol(INode? node);
}

