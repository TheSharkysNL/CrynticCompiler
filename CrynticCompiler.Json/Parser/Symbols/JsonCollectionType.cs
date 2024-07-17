namespace CrynticCompiler.Json.Parser.Symbols;

public class JsonCollectionType : ExtendableEnum
{
    public static readonly int List = AutoIncrement();

    public static readonly int Object = AutoIncrement();
}