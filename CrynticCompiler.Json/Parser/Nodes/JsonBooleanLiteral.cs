using System.Numerics;
using System.Text;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;
using CrynticCompiler.Extensions;

namespace CrynticCompiler.Json.Parser.Nodes;

public class JsonBooleanLiteral<TData>(bool truthy, INode? parent = null) : JsonLiteral<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public static readonly TokenData<TData> TrueLiteral = new("true".AsSpan().Convert<char, TData>());
    public static readonly TokenData<TData> FalseLiteral = new("false".AsSpan().Convert<char, TData>());
    
    public override TokenData<TData> Literal => Truthy ? TrueLiteral : FalseLiteral;

    public bool Truthy => truthy;

    public static readonly JsonBooleanLiteral<TData> TruthyInstance = new(true);
    public static readonly JsonBooleanLiteral<TData> FalsyInstance = new(false);

    public static JsonBooleanLiteral<TData> GetInstance(bool truthy) =>
        truthy ? TruthyInstance : FalsyInstance;

    public override string ToString() =>
        truthy ? "true" : "false";

    public override void AppendString(StringBuilder builder) =>
        builder.Append(ToString());
}