using System.Collections;
using CrynticCompiler.Extensions;

namespace CrynticCompiler.Collections;

public readonly struct ReadOnlyArray<T>(T[] array, int count) : IReadOnlyList<T>
{
    public int Count => count;

    public T this[int index] => array[index];

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
        {
            yield return array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}