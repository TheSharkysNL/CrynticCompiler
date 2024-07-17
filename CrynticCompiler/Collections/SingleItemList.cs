using System.Collections;

namespace CrynticCompiler.Collections;

public readonly struct SingleItemList<T> : IReadOnlyList<T>, IList<T>
{
    private readonly T value; 

    public int Count => 1;

    public bool IsReadOnly => true;

    public SingleItemList(T value)
    {
        this.value = value;
    }

    public T this[int index]
    {
        get
        {
            if (index != 0)
                throw new IndexOutOfRangeException();
            return value;
        }
        set => throw new InvalidOperationException();
    }

    public IEnumerator<T> GetEnumerator() =>
        new ValueEnumerator<T>(value);

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public int IndexOf(T item)
    {
        if (item is null)
        {
            if (value is null)
                return 0;
            return -1;
        }
        if (item.Equals(value))
            return 0;
        return -1;
    }

    public void Insert(int index, T item) =>
        throw new InvalidOperationException();

    public void RemoveAt(int index) =>
        throw new InvalidOperationException();

    public void Add(T item) =>
        throw new InvalidOperationException();

    public void Clear() =>
        throw new InvalidOperationException();

    public bool Contains(T item)
    {
        if (item is null)
        {
            if (value is null)
                return true;
            return false;
        }
        return item.Equals(value);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if ((uint)array.Length <= (uint)arrayIndex)
            throw new ArgumentException($"the {nameof(array)} length is not long enough to supply items at the {nameof(arrayIndex)}");

        array[arrayIndex] = value;
    }

    public bool Remove(T item) =>
        throw new InvalidOperationException();
}
