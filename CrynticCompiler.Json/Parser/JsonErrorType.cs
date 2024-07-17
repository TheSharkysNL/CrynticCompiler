using CrynticCompiler.Parser;

namespace CrynticCompiler.Json.Parser;

public class JsonErrorType : ErrorType
{
    public static readonly int ListNeverClosed = AutoIncrement();

    public static readonly int ObjectNeverClosed = AutoIncrement();

    public static readonly int TrailingSeparatorNotAllowed = AutoIncrement();
}