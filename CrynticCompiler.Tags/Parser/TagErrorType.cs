using CrynticCompiler.Parser;

namespace CrynticCompiler.Tags.Parser;

public class TagErrorType : ErrorType
{
    public static readonly int ExpectedATag = AutoIncrement();
    
    public static readonly int TagNameNotFound = AutoIncrement();

    public static readonly int TagNotClosed = AutoIncrement();

    public static readonly int ExpectedAttributeValue = AutoIncrement();

    public static readonly int OpeningAndClosingTagsDontMatch = AutoIncrement();
}