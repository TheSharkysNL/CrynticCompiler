using System.Numerics;
using CrynticCompiler.Extensions;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Nodes;

public class IdentifierLiteral<TData>(TokenData<TData> name, INode? parent = null) : ILiteralExpression<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public readonly TokenData<TData> Name = name;

    public TokenData<TData> Literal => Name;
    
    public INode? Parent { get; } = parent;

    public override string ToString() =>
        name.Span.ConvertToCharacters();
}