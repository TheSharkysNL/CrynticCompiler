using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CrynticCompiler.Extensions;
using ArgumentException = System.ArgumentException;

namespace CrynticCompiler.Collections;

public class SortedList<T> : IReadOnlyList<T>, IList<T>
{
    private T[] array;
    private int count;

    public int Count => count;
    public bool IsReadOnly => false;

    public int Capacity => array.Length;

    private readonly IComparer<T> comparer;

    public SortedList(IComparer<T>? comparer = null)
        : this(0, comparer)
    { }

    public SortedList(Func<T?, T?, int> comparer)
        : this(0, comparer)
    { }

    public SortedList(int capacity, IComparer<T>? comparer = null)
    {
        array = GetArray(capacity);
        this.comparer = comparer ?? Comparer<T>.Default;
    }
    
    public SortedList(int capacity, Func<T?, T?, int> comparer)
    {
        array = GetArray(capacity);
        this.comparer = new FuncComparer<T>(comparer);
    }

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)count)
            {
                throw new IndexOutOfRangeException();
            }

            return array[index];
        }
        set
        {
            if ((uint)index >= (uint)count)
            {
                throw new IndexOutOfRangeException();
            }

            if (index == 0)
            {
                int valueIndex = BisectRight(value, array, 1, count - 1, comparer) - 1;
                Array.Copy(array, 1, array, 0, valueIndex);
                array[valueIndex] = value;
                return;
            }

            RemoveAt(index);
            Add(value);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
        {
            yield return array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(T item)
    {
        int count = this.count;
        int newCount = count + 1;

        ResizeIfNeeded(newCount);

        Insert(item, array, count, comparer);
        this.count = newCount;
    }

    public void AddRange(T[] items) =>
        AddRange((IReadOnlyList<T>)items);

    public void AddRange(IReadOnlyList<T> items)
    {
        int count = this.count;
        int newCount = count + items.Count;
        
        ResizeIfNeeded(newCount);

        T[] array = this.array;
        IComparer<T> comparer = this.comparer;
        int itemsCount = items.Count;
        for (int i = 0; i < itemsCount; i++)
        {
            Insert(items[i], array, count + i, comparer);
        }

        this.count = newCount;
    }

    private static int BisectRight(T value, T[] array, int start, int count, IComparer<T> comparer)
    {
        int low = start;
        int high = count;

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (comparer.Compare(value, array[mid]) < 0)
            {
                high = mid;
            }
            else
            {
                low = mid + 1;
            }
        }

        return low;
    }

    private static void Insert(T value, T[] array, int count, IComparer<T> comparer)
    {
        Debug.Assert(array.Length >= count + 1);
        int index = BisectRight(value, array, 0, count, comparer);
        Debug.Assert(index >= 0);
        
        Array.Copy(array, index, array, index + 1, count - index);
        array[index] = value;
    }

    public void Clear()
    {
        count = 0;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(array);
        }
    }

    public bool Contains(T item) =>
        IndexOf(item) >= 0;

    public void CopyTo(T[] array, int arrayIndex)
    {
        if ((uint)arrayIndex >= array.Length)
        {
            throw new IndexOutOfRangeException();
        }
        if (count + arrayIndex > array.Length)
        {
            throw new ArgumentException("array is too small to copy to", nameof(array));
        }
        
        Array.Copy(this.array, 0, array, arrayIndex, count);
    }

    public bool Remove(T item)
    {
        int index = Array.BinarySearch(array, 0, count, item, comparer);
        if (index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    public int IndexOf(T item)
    {
        int index = Array.BinarySearch(array, 0, count, item, comparer);
        if (index < 0)
        {
            return -1;
        }

        return index;
    }

    void IList<T>.Insert(int index, T item)
    {
        throw new InvalidOperationException("cannot insert a value into a sorted list");
    }

    public void RemoveAt(int index)
    {
        int newCount = --count;
        Array.Copy(array, index + 1, array, index, newCount - index);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            array[newCount + 1] = default!;
        }
    }

    private static T[] GetArray(int capacity)
    {
        if (capacity <= 0)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("capacity cannot be less than 0", nameof(capacity));
            }

            return [];
        }
        
        return new T[capacity];
    }
    
    private void ResizeIfNeeded(int newCount){
        if (newCount >= array.Length)
        {
            T[] newArr = new T[newCount * 2];
            array.CopyTo(newArr, 0);
            array = newArr;
        }
    }
}