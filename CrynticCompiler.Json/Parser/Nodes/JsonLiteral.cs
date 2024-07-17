using System.Numerics;
using System.Text;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Nodes;

public abstract class JsonLiteral<TData>(INode? parent = null) : IJsonNode, ILiteralExpression<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public INode? Parent { get; } = parent;
    
    public virtual void AppendString(StringBuilder builder)
    {
        ReadOnlySpan<TData> literal = Literal.Span;
        int length = literal.Length;
        for (int i = 0; i < length; i++)
        {
            char c = (char)ushort.CreateTruncating(literal[i]);
            builder.Append(c);
        }
    }

    public void AppendStringFormatted(StringBuilder builder, int indent = 0)
    {
        builder.Append(' ', indent);
        AppendString(builder);
    }

    public abstract TokenData<TData> Literal { get; }

    public override string ToString() =>
        Literal.ToString();
}