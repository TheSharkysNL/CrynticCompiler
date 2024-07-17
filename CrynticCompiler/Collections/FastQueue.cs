using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrynticCompiler.Collections;

public sealed class FastQueue<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private T[] array;
    private int count;

    public int Count => count;
    // {
    //     int tail = this.tail;
    //     int head = this.head;
    //     int arrayLength = array.Length;
    //
    //     int tailMinHead = tail - head;
    //     return (tailMinHead & ((head - tail) >> 31)) + ((arrayLength - head + tail) & (tailMinHead >> 31));
    //     /*
    //      * same as 
    //      * if (tail < head)
    //      *  return tail - head;
    //      * else
    //      *  return arrayLength - head + tail;
    //      */
    // }
    
    public int Capacity
    {
        get => array.Length;
        set
        {
            int count = this.count;
            if (value < count)
                throw new ArgumentOutOfRangeException(nameof(value), "capacity cannot be smaller than the current count");
            SetCapacity(count, value);
        }
    }

    private int head;
    private int tail;

    public bool IsReadOnly => false;

    public FastQueue()
    {
        array = [];
    }

    public FastQueue(int capacity)
    {
        if (capacity <= 0)
        {
            if (capacity == 0)
                array = [];
            else
                throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        else
            array = new T[capacity];
    }

    public void Enqueue(T item)
    {
        int count = this.count;
        int newCount = count + 1;
        if (newCount >= array.Length)
            IncreaseSize(count, newCount);

        T[] arr = array;
        arr[tail] = item;
        tail = NextPosition(tail);
        this.count = newCount;
    }

    public void Enqueue(ReadOnlySpan<T> items)
    {
        int count = this.count;
        int itemsLength = items.Length; 
        int newCount = count + itemsLength;
        if (newCount >= array.Length)
            IncreaseSize(count, newCount);

        T[] arr = array;
        int tail = this.tail;
        int head = this.head;

        if (tail < head)
        {
            int firstCopyLength = arr.Length - head;
            items[..firstCopyLength].CopyTo(arr.AsSpan(head, firstCopyLength));
            items[firstCopyLength..].CopyTo(arr.AsSpan(0, tail));
        }
        else
            items.CopyTo(arr.AsSpan(head, itemsLength));

        tail += itemsLength;
        if (tail >= arr.Length)
            this.tail = tail - arr.Length;
        else
            this.tail = tail;
        this.count = newCount;
    }


    public void Enqueue(ICollection<T> collection)
    {
        if (collection is T[] arr)
            Enqueue(arr.AsSpan());
        else if (collection is List<T> list)
            Enqueue(CollectionsMarshal.AsSpan(list));
        else
        {
            int count = this.count;
            int newCount = count + collection.Count;
            if (newCount >= array.Length)
                IncreaseSize(count, newCount);
            EnqueueEnumerable(collection);
            this.count = newCount;
        }
    }


    public void Enqueue(IReadOnlyCollection<T> collection)
    {
        if (collection is T[] arr)
            Enqueue(arr.AsSpan());
        else if (collection is List<T> list)
            Enqueue(CollectionsMarshal.AsSpan(list));
        else if (collection is ImmutableArray<T> imArr)
            Enqueue(imArr.AsSpan());
        else
        {
            int count = this.count;
            int newCount = count + collection.Count;
            if (newCount >= array.Length)
                IncreaseSize(count, newCount);
            EnqueueEnumerable(collection);
            this.count = newCount;
        }
    }

    public void Enqueue(IEnumerable<T> enumerable)
    {
        if (enumerable is IReadOnlyCollection<T> readonlyCollection)
            Enqueue(readonlyCollection);
        else if (enumerable is ICollection<T> collection)
            Enqueue(collection);
        else
            EnqueueEnumerable(enumerable);
    }

    private void EnqueueEnumerable(IEnumerable<T> values)
    {
        int num = 0;
        foreach (T value in values)
        {
            Enqueue(value);
            num++;
        }
        count += num; 
    }

    public T Dequeue()
    {
        T[] arr = array;
        int head = this.head;
        int count = this.count;

        if (count <= 0)
            throw new InvalidOperationException("cannot dequeue an item from an empty queue");

        T removed = arr[head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            arr[head] = default!;

        this.head = NextPosition(head);
        this.count--;

        return removed;
    }

    public bool TryDequeue(out T value)
    {
        T[] arr = array;
        int head = this.head;
        int count = this.count;

        if (count <= 0)
        {
            value = default!;
            return false;
        }

        T removed = arr[head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            arr[head] = default!;

        this.head = NextPosition(head);
        this.count--;

        value = removed;
        return true;
    }

    public bool RemoveLast(int count)
    {
        T[] arr = array;
        int head = this.head;

        int collectionCount = this.count;

        if (collectionCount < count | count == 0)
            return false;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(arr, head, head + count < arr.Length ? count : arr.Length - head);
            if (head + count >= arr.Length)
            {
                Array.Clear(arr, 0, head);
            }
        }

        head += count;
        if (head >= arr.Length)
            head -= arr.Length;
        this.head = head; 
        this.count -= count;
        
        

        return true;
    }

    public T Peek()
    {
        T[] arr = array;
        int head = this.head;
        int count = this.count;

        if (count == 0)
            throw new InvalidOperationException("cannot peek an item from an empty queue");

        return arr[head];
    }

    public bool TryPeek(out T value)
    {
        T[] arr = array;
        int head = this.head;
        int count = this.count;

        if (count == 0)
        {
            value = default!;
            return false;
        }

        value = arr[head];
        return true;
    }

    void ICollection<T>.Add(T item) =>
        Enqueue(item);

    public void Clear()
    {
        int tail = this.tail;
        int head = this.head;
        T[] arr = array;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (tail < head)
            {
                Array.Clear(arr, head, arr.Length - head);
                Array.Clear(arr, 0, tail);
            }
            else
                Array.Clear(arr, head, tail - head);
        }

        this.tail = 0;
        this.head = 0;
        this.count = 0;
    }

    public bool Contains(T item)
    {
        T[] arr = array;
        int head = this.head;
        int tail = this.tail;

        if (tail < head)
            return Array.IndexOf(arr, item, head, arr.Length - head) >= 0 ||
                Array.IndexOf(arr, item, 0, tail) >= 0;

        return Array.IndexOf(arr, item, head, tail - head) >= 0;

    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        T[] arr = this.array;
        int head = this.head;
        int tail = this.tail;

        if ((uint)arrayIndex > array.Length)
            throw new IndexOutOfRangeException(nameof(arrayIndex));

        int count = this.count;
        if (array.Length - arrayIndex < count)
            throw new ArgumentException("array at index does not contain enough entries to fit this collection");

        if (tail < head)
        {
            int firstCopyLength = array.Length - head;
            Array.Copy(arr, head, array, 0, firstCopyLength);
            Array.Copy(arr, 0, array, firstCopyLength, tail);
        }
        else
            Array.Copy(arr, head, array, 0, count);
    }

    public IEnumerator<T> GetEnumerator()
    {
        T[] array = this.array;
        int arrayLength = array.Length;

        int tail = this.tail;
        int head = this.head;

        if (tail < head)
        {
            for (int i = head; i < arrayLength; i++)
                yield return array[i];

            for (int i = 0; i < tail; i++)
                yield return array[i];
        }
        else
        {
            for (int i = head; i < tail; i++)
                yield return array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void SetCapacity(int count, int capacity)
    {
        Debug.Assert(capacity >= count);
        T[] currArray = array;
        T[] newArray = new T[capacity];

        int head = this.head;
        int tail = this.tail;

        if (tail < head)
        {
            int firstCopyLength = currArray.Length - head;
            Array.Copy(currArray, head, newArray, 0, firstCopyLength);
            Array.Copy(currArray, 0, newArray, firstCopyLength, tail);
        }
        else
        {
            Array.Copy(currArray, head, newArray, 0, count);
        }

        this.array = newArray;

        bool isNotAtCapacity = count != capacity;
        this.tail = count * Unsafe.As<bool, byte>(ref isNotAtCapacity);
        this.head = 0;
    }

    private void IncreaseSize(int count, int newSize)
    {
        SetCapacity(count, newSize * 2);
    }

    private int NextPosition(int value)
    {
        value += 1;
        bool isNotAtEnd = value != array.Length;
        return value * Unsafe.As<bool, byte>(ref isNotAtEnd);
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotImplementedException(); // impossible
    }
}

