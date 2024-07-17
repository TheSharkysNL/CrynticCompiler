using System.Numerics;
using System.Text;
using CrynticCompiler.Extensions;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Nodes;

public class JsonNullLiteral<TData>(INode? parent = null) : JsonLiteral<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public static readonly TokenData<TData> NullLiteral = new("null".AsSpan().Convert<char, TData>());
    public override TokenData<TData> Literal => NullLiteral;

    public static readonly JsonNullLiteral<TData> Instance = new();

    public override string ToString() =>
        "null";

    public override void AppendString(StringBuilder builder) =>
        builder.Append(ToString());
}