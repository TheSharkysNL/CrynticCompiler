using System.Collections;
using System.Numerics;
using CrynticCompiler.Json.Parser.Nodes;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Symbols;

public abstract class JsonCollectionSymbol<TData>(
    IReadOnlyList<IJsonNode> nodes,
    ISemanticModel<TData> model,
    ISymbol<TData>? parent = null) : ISymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ISymbol<TData>? Parent => parent;
    
    public TokenData<TData> Name => TokenData<TData>.Empty;

    public abstract int CollectionType { get; }

    public int Count => nodes.Count;

    private ISymbol<TData>?[]? computedSymbols;
    protected bool TryGetComputedSymbol(int index, out ISymbol<TData>? symbol)
    {
        if (computedSymbols is null || (uint)index >= (uint)computedSymbols.Length)
        {
            symbol = null;
            return false;
        }

        symbol = computedSymbols[index];
        return symbol is not null;
    }

    protected void SetComputedSymbol(int index, ISymbol<TData>? symbol)
    {
        if (computedSymbols is null || (uint)index >= (uint)computedSymbols.Length)
        {
            ISymbol<TData>?[] temp = new ISymbol<TData>?[(index + 1) * 3 / 2];
            computedSymbols?.CopyTo(temp, 0);
            computedSymbols = temp;
        }
            
        computedSymbols[index] = symbol;
    }

    protected void SetComputedSymbolsSize(int size)
    {
        if (computedSymbols is null)
        {
            computedSymbols = new ISymbol<TData>[size];
            return;
        }

        if (size >= computedSymbols.Length)
        {
            ISymbol<TData>?[] temp = new ISymbol<TData>?[size];
            computedSymbols?.CopyTo(temp, 0);
            computedSymbols = temp;
        }
    }
    
    public ISymbol<TData>? this[int index]
    {
        get
        {
            if ((uint)index >= (uint)nodes.Count)
            {
                throw new IndexOutOfRangeException();
            }

            if (TryGetComputedSymbol(index, out ISymbol<TData>? computedSymbol))
            {
                return computedSymbol;
            }
            
            INode node = nodes[index];
            ISymbol<TData>? symbol = model.GetSymbol(node);

            SetComputedSymbol(index, symbol);
            return symbol;
        }
    }

    public ICollection GetCollection()
    {
        int nodesCount = Count;
        SetComputedSymbolsSize(nodesCount);
        return CreateCollection(nodesCount);
    }

    protected virtual ICollection CreateCollection(int nodesCount)
    {
        object?[] objects = new object[nodesCount];

        for (int i = 0; i < nodesCount; i++)
        {
            ISymbol<TData>? symbol = this[i];
            objects[i] = GetObject(symbol);
        }

        return objects;
    }

    protected virtual object? GetObject(ISymbol<TData>? symbol)
    {
        if (symbol is null)
        {
            return null;
        }

        if (symbol is JsonCollectionSymbol<TData> collection)
        {
            return collection.GetCollection();
        }

        if (symbol is ILiteralSymbol<TData> literal)
        {
            return literal.Literal;
        }

        return null;
    }
}