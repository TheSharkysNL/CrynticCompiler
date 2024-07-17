using System.Numerics;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Parser.Symbols;

public class IdentifierSymbol<TData>(TokenData<TData> name, SemanticModel<TData> model, ISymbol<TData>? parent = null) : LiteralSymbol<TData>(parent)
    where TData : unmanaged, IBinaryInteger<TData>
{
    public override TokenData<TData> Name => name;

    public override object? Literal { get; } // TODO: implement using model to get value
    
    public override int Type => LiteralType.Identifier;
}