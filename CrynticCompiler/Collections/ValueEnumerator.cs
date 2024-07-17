using System.Collections;

namespace CrynticCompiler.Collections;

internal sealed class ValueEnumerator<T> : IEnumerator<T>, IEnumerable<T>
{
    private bool hasNotGivenValue = true;
    private T value;
    public T Current => value;

    object IEnumerator.Current => value!;

    public ValueEnumerator(T value)
    {
        this.value = value;
    }

    public void Dispose()
    {
        value = default!;
    }

    public bool MoveNext()
    {
        bool b = hasNotGivenValue;
        hasNotGivenValue = false;
        return b;
    }

    public void Reset()
    {
        hasNotGivenValue = true;
    }

    public IEnumerator<T> GetEnumerator() =>
        this;

    IEnumerator IEnumerable.GetEnumerator() =>
        this;
}
