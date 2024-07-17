using System.Numerics;
using CrynticCompiler.Json.Parser.Nodes;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Symbols;

namespace CrynticCompiler.Json.Parser.Symbols;

public class JsonListSymbol<TData>(
    IReadOnlyList<IJsonNode> nodes,
    ISemanticModel<TData> model,
    ISymbol<TData>? parent = null)
    : JsonCollectionSymbol<TData>(nodes, model, parent) 
    where TData : unmanaged, IBinaryInteger<TData>
{
    public override int CollectionType => JsonCollectionType.List;
}