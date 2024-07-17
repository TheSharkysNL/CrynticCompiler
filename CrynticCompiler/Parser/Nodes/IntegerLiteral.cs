using System;
using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Extensions;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Nodes;

[DebuggerDisplay("{CrynticCompiler.Extensions.MemoryExtensions.ConvertToCharacters(Literal.Span), nq}")]
public class IntegerLiteral<TData>(TokenData<TData> literal, int type, INode? parent = null) : ILiteralExpression<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public TokenData<TData> Literal { get; } = literal;
    
    public INode? Parent { get; } = parent;

    public int Type = type;

    public override string ToString() =>
        Literal.ToString();
}

