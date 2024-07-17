using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Json.Parser.Nodes;
using CrynticCompiler.Json.Parser.Symbols;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser;

public class JsonSemanticModel<TData>(IParseTree<TData> tree) : SemanticModel<TData>(tree) 
    where TData : unmanaged, IBinaryInteger<TData>
{
    protected override ILiteralSymbol<TData>? GetLiteralSymbol(ILiteralExpression<TData> literal, ISymbol<TData>? parent)
    {
        if (literal is JsonIntegerLiteral<TData> num)
        {
            if (num.Type == TokenType.Float)
            {
                return new FloatSymbol<TData>(num.Literal, parent);
            }

            return new IntegerSymbol<TData>(num.Literal, num.Type, parent);
        }

        if (literal is JsonStringLiteral<TData> str)
        {
            return new StringSymbol<TData>(str.Literal, parent);
        }

        if (literal is JsonBooleanLiteral<TData> @bool)
        {
            return new JsonBooleanSymbol<TData>(@bool.Truthy, parent);
        }

        if (literal is JsonNullLiteral<TData>)
        {
            return new JsonNullSymbol<TData>(parent);
        }

        return null;
    }

    protected override ISymbol<TData>? OnNodeNotFound(INode node, ISymbol<TData>? parent)
    {
        if (node is JsonCollectionNode collection)
        {
            if (collection is KeyValueMapNode<TData> map)
            {
                return new JsonObjectSymbol<TData>(map.Pairs, this, parent);
            }
            
            Debug.Assert(collection is ListNode);
            return new JsonListSymbol<TData>(collection.Items, this, parent);
        }

        if (node is KeyValuePairNode<TData> pair)
        {
            return new KeyValuePairSymbol<TData>(pair.Key, pair.Value, this, parent);
        }

        return null;
    }
}