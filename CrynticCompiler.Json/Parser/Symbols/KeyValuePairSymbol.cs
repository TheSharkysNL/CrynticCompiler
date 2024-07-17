using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CrynticCompiler.Json.Parser.Nodes;
using CrynticCompiler.Parser;
using CrynticCompiler.Parser.Symbols;
using CrynticCompiler.Tokenizer;

namespace CrynticCompiler.Json.Parser.Symbols;

public class KeyValuePairSymbol<TData>(
    Token<TData> key,
    IJsonNode value,
    ISemanticModel<TData> model,
    ISymbol<TData>? parent = null) : ISymbol<TData>
    where TData : unmanaged, IBinaryInteger<TData>
{
    public ISymbol<TData>? Parent => parent;
    
    public TokenData<TData> Name => key.Data;

    private string? keyBuffer;
    public string Key
    {
        get
        {
            if (keyBuffer is not null)
            {
                return keyBuffer;
            }
            
            keyBuffer = GetKey();
            return keyBuffer;
        }
    }

    protected virtual string GetKey()
    {
        Encoding encoding = StringSymbol<TData>.GetEncoding();
            
        ReadOnlySpan<TData> span = key.Data.Span;
        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(span);
        string encodedString = encoding.GetString(bytes);
        return encodedString;
    }

    private ISymbol<TData>? valueSymbol;
    public ISymbol<TData>? Value => valueSymbol ??= model.GetSymbol(value);
}