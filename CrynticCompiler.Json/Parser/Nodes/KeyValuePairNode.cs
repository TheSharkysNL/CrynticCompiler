using System.Numerics;
using System.Text;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;
using Microsoft.Extensions.Primitives;

namespace CrynticCompiler.Json.Parser.Nodes;

public class KeyValuePairNode<TData>(Token<TData> key, IJsonNode val, INode? parent = null) : IJsonNode
    where TData : unmanaged, IBinaryInteger<TData>
{
    public Token<TData> Key => key;
    public IJsonNode Value
    {
        get => val;
        protected internal set => val = value;
    }
    
    public INode? Parent { get; } = parent;

    public override string ToString() => Key + ":" + Value;
    
    public void AppendString(StringBuilder builder)
    {
        builder.Append(key.Data);
        builder.Append(':');
        Value.AppendString(builder);
    }

    public void AppendStringFormatted(StringBuilder builder, int indent = 0)
    {
        builder.Append(' ', indent);
        builder.Append(key.Data);
        builder.Append(':');
        if (Value is JsonCollectionNode)
        {
            builder.Append('\n');
            Value.AppendStringFormatted(builder, indent);
            return;
        }
        Value.AppendString(builder);
    }
}