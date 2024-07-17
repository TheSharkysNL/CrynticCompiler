using System.Numerics;
using CrynticCompiler.Json.Parser.Nodes;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Symbols;

public class JsonObjectSymbol<TData>(
    IReadOnlyList<KeyValuePairNode<TData>> nodes,
    ISemanticModel<TData> model,
    ISymbol<TData>? parent = null)
    : JsonCollectionSymbol<TData>(nodes, model, parent) 
    where TData : unmanaged, IBinaryInteger<TData>
{
    private readonly ISemanticModel<TData> model1 = model;

    public bool TryGetSymbol(TokenData<TData> key, out ISymbol<TData>? symbol)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            KeyValuePairNode<TData> node = nodes[i];
            if (node.Key.Data.Span.SequenceEqual(key.Span))
            {
                if (TryGetComputedSymbol(i, out symbol))
                {
                    return true;
                }
                    
                symbol = model1.GetSymbol(node);
                SetComputedSymbol(i, symbol);
                return true;
            }
        }

        symbol = null;
        return false;
    }
    
    public ISymbol<TData>? this[TokenData<TData> key]
    {
        get
        {
            if (TryGetSymbol(key, out ISymbol<TData>? symbol))
            {
                return symbol;
            }

            throw new KeyNotFoundException();
        }
    }

    public override int CollectionType => JsonCollectionType.Object;

    protected override Dictionary<string, object?> CreateCollection(int nodesCount)
    {
        Dictionary<string, object?> dictionary = new(nodesCount);

        for (int i = 0; i < nodesCount; i++)
        {
            ISymbol<TData>? symbol = this[i];
            KeyValuePair<string, object?> pair = GetPair(symbol);
            dictionary.Add(pair.Key, pair.Value);
        }

        return dictionary;
    }
    
    protected sealed override object GetObject(ISymbol<TData>? symbol) =>
        GetPair(symbol);

    protected virtual KeyValuePair<string, object?> GetPair(ISymbol<TData>? symbol)
    {
        if (symbol is not KeyValuePairSymbol<TData> pair)
        {
            return new(string.Empty, null);
        }

        string key = pair.Key;
        object? value = base.GetObject(pair.Value);

        return new(key, value);
    }
}