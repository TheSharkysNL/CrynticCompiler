using System;
using System.Collections;

namespace CrynticCompiler.Collections;

internal readonly struct EmptyEnumerator<T> : IEnumerator<T>
{
    public static readonly EmptyEnumerator<T> Instance = new();

    public T Current => default!;

    object IEnumerator.Current => default!;

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
        return false;
    }

    public void Reset()
    {
    }
}

