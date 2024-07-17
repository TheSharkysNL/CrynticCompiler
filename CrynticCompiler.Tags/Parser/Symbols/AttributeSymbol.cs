using System.Numerics;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Nodes;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Tags.Parser.Symbols;

public class AttributeSymbol<TData>(ILiteralExpression<TData> name, INode value, ISemanticModel<TData> model, ISymbol<TData>? parent = null) : ISymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ISymbol<TData>? Parent => parent;
    public TokenData<TData> Name => name.Literal;
    
    private ISymbol<TData>? val;
    public ISymbol<TData>? Value => val ??= model.GetSymbol(value);
}