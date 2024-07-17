using System.Numerics;
using CrynticCompiler.Extensions;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Tags.Parser.Nodes;

public class TagAttribute<TData>(ILiteralExpression<TData> name, INode val, INode? parent = null) : INode
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ILiteralExpression<TData> Name
    {
        get => name;
        protected internal set => name = value;
    }

    public INode Value
    {
        get => val;
        protected internal set => val = value;
    }

    public INode? Parent { get; } = parent; 

    public override string ToString() =>
        $"{Name}={Value}";
}