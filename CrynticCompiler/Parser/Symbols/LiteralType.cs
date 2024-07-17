namespace CrynticCompiler.Parser.Symbols;

public class LiteralType : ExtendableEnum
{
    public static readonly int String = AutoIncrement();

    public static readonly int Integer = AutoIncrement();

    public static readonly int Float = AutoIncrement();

    public static readonly int Identifier = AutoIncrement();
}