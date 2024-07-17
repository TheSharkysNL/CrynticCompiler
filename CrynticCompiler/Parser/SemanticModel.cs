using System.Diagnostics;
using System.Numerics;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser;

public abstract class SemanticModel<TData>(IParseTree<TData> tree) : ISemanticModel<TData>
	where TData : unmanaged, IBinaryInteger<TData>
{
	private readonly Dictionary<INode, ISymbol<TData>?> computedNodes = new(4);
	
	public ISymbol<TData>? GetSymbol(INode? node)
	{
		if (node is null)
		{
			return null;
		}

		if (computedNodes.TryGetValue(node, out ISymbol<TData>? computedNode))
		{
			return computedNode;
		}

		ISymbol<TData>? symbol = CreateSymbol(node);
		computedNodes.Add(node, symbol);
		return symbol;
	}
	
	protected virtual ILiteralSymbol<TData>? GetLiteralSymbol(ILiteralExpression<TData> literal, ISymbol<TData>? parent)
	{
		if (literal is IntegerLiteral<TData> num)
		{
			if (num.Type == TokenType.Float)
			{
				return new FloatSymbol<TData>(num.Literal, parent);
			}

			return new IntegerSymbol<TData>(num.Literal, num.Type, parent);
		}

		if (literal is StringLiteral<TData> str)
		{
			return new StringSymbol<TData>(str.Literal, parent);
		}

		if (literal is IdentifierLiteral<TData> identifier)
		{
			return new IdentifierSymbol<TData>(identifier.Literal, this, parent);
		}

		return null;
	}

	protected virtual IExpressionSymbol<TData>? GetExpressionSymbol(IExpression<TData> expression,
		ISymbol<TData>? parent)
	{
		return null;
	}

	protected ISymbol<TData>? CreateSymbol(INode node)
	{
		Debug.Assert(node is not null);
		ISymbol<TData>? parent = GetSymbol(node.Parent);
		
		if (node is IExpression<TData> expression)
		{
			if (expression is ILiteralExpression<TData> literal)
			{
				return GetLiteralSymbol(literal, parent);
			}

			return GetExpressionSymbol(expression, parent);
		}

		return OnNodeNotFound(node, parent);
	}

	protected abstract ISymbol<TData>? OnNodeNotFound(INode node, ISymbol<TData>? parent);
}

