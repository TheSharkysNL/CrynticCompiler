namespace CrynticCompiler.Collections;

public sealed class FuncComparer<T>(Func<T?, T?, int> comparer) : IComparer<T>
{
    public int Compare(T? x, T? y) =>
        comparer(x, y);
}