using System.Numerics;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tags.Parser.Nodes;
using CrynticCompiler.Tags.Parser.Symbols;

namespace CrynticCompiler.Tags.Parser;

public class TagSemanticModel<TData>(IParseTree<TData> tree) : SemanticModel<TData>(tree) where TData : unmanaged, IBinaryInteger<TData>
{
    protected override ISymbol<TData>? OnNodeNotFound(INode node, ISymbol<TData>? parent)
    {
        if (node is Tag<TData> tag)
        {
            return new TagSymbol<TData>(tag.Name, tag.Attributes, tag.Children, this, parent);
        }

        if (node is TagAttribute<TData> attribute)
        {
            return new AttributeSymbol<TData>(attribute.Name, attribute.Value, this, parent);
        }

        return null;
    }
}