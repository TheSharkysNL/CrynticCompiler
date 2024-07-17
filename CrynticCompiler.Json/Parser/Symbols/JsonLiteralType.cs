using CrynticCompiler.Parser.Symbols;

namespace CrynticCompiler.Json.Parser.Symbols;

public class JsonLiteralType : LiteralType
{
    public static readonly int Null = AutoIncrement();

    public static readonly int Bool = AutoIncrement();
}