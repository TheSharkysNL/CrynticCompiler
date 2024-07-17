using System;
using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Symbols;

public interface ISymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ISymbol<TData>? Parent { get; }
    
    /// <summary>
    /// the name of the symbol can be <see cref="TokenData{TData}.Empty"/> if symbol has no name
    /// </summary>
    public TokenData<TData> Name { get; }
}

