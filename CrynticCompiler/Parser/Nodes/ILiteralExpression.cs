using System;
using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Nodes;

public interface ILiteralExpression<TData> : IExpression<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public TokenData<TData> Literal { get; }
}

