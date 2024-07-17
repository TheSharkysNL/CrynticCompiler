using System.Numerics;
using System.Text;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Nodes;

public class JsonStringLiteral<TData>(TokenData<TData> @string, INode? parent = null) : JsonLiteral<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public override TokenData<TData> Literal => @string;

    public override void AppendString(StringBuilder builder)
    {
        builder.Append('\"');
        base.AppendString(builder);
        builder.Append('\"');
    }

    public override string ToString() =>
        "\"" + Literal + "\"";
}