namespace CrynticCompiler.Parser;

public class ErrorType : ExtendableEnum
{
    public static readonly int ExpectedIdentifier = AutoIncrement();

    public static readonly int ExpectedValue = AutoIncrement();

    public static readonly int ExpectedString = AutoIncrement();

    public static readonly int ExpectedExpression = AutoIncrement();

    public static readonly int ExpectedExpressionAfter = AutoIncrement();
}