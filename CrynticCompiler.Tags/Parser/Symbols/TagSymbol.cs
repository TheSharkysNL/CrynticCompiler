using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tags.Parser.Nodes;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Tags.Parser.Symbols;

public class TagSymbol<TData>(
    TokenData<TData> name,
    IReadOnlyList<TagAttribute<TData>> attributes,
    IReadOnlyList<Tag<TData>> children,
    ISemanticModel<TData> model,
    ISymbol<TData>? parent = null) : ISymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ISymbol<TData>? Parent => parent;
    public TokenData<TData> Name => name;

    public int AttributeCount => attributes.Count;
    public int ChildCount => children.Count;

    private TSymbol GetSymbol<TSymbol, TNode>(int index, IReadOnlyList<TNode> nodes, TSymbol?[] symbols)
        where TSymbol : ISymbol<TData>
        where TNode : INode
    {
        if ((uint)index >= (uint)nodes.Count)
        {
            throw new IndexOutOfRangeException();
        }
        
        TSymbol? atIndex = symbols[index];
        if (atIndex is not null)
        {
            return atIndex;
        }

        TNode node = nodes[index];
        ISymbol<TData>? symbol = model.GetSymbol(node);
        
        Debug.Assert(symbol is TSymbol);
        symbols[index] = (TSymbol)symbol;

        return (TSymbol)symbol;
    }

    private readonly AttributeSymbol<TData>?[] attributeSymbols = attributes.Count == 0 ? [] : new AttributeSymbol<TData>?[attributes.Count];

    public AttributeSymbol<TData> GetAttribute(int index) =>
        GetSymbol(index, attributes, attributeSymbols);

    private readonly TagSymbol<TData>?[] childSymbols = children.Count == 0 ? [] : new TagSymbol<TData>?[children.Count];
    public TagSymbol<TData> GetChild(int index) =>
        GetSymbol(index, children, childSymbols);
}