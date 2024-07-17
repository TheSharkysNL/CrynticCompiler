using System.Numerics;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Nodes;

public class JsonIntegerLiteral<TData>(TokenData<TData> literal, int type, INode? parent = null) : JsonLiteral<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public override TokenData<TData> Literal => literal;

    public int Type => type;
}