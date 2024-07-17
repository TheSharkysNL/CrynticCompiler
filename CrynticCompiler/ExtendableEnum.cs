namespace CrynticCompiler;

public abstract class ExtendableEnum
{
    private static int currentNum;

    protected static int AutoIncrement() =>
        currentNum++;
}