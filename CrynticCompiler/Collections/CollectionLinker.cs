using System.Collections;
using System.Diagnostics;

namespace CrynticCompiler.Collections;

public sealed class CollectionLinker<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private int count;
    public int Count => count;

    public bool IsReadOnly => false;

    private readonly CollectionLinkerNode Head;
    private CollectionLinkerNode Tail;

    public CollectionLinker()
    {
        Head = new();
        Tail = Head;
    }
    
    public void Add(T item)
    {
        AddEnumerable(new ValueEnumerator<T>(item));

        count++;
    }

    public void Add(ICollection<T> collection)
    {
        AddEnumerable(collection);

        count += collection.Count;
    }

    public void Add(IReadOnlyCollection<T> collection)
    {
        AddEnumerable(collection);

        count += collection.Count;
    }

    private void AddEnumerable(IEnumerable<T> values)
    {
        Tail.Values = values;
        CollectionLinkerNode next = new(null, Tail);
        Tail.Next = next;
        Tail = next;
    }

    public void Clear()
    {
        CollectionLinkerNode node = Head;
        while (node.Values is not null)
        {
            CollectionLinkerNode temp = node;
            node = node.Next!;
            temp.Dispose();
        }

        Tail = Head;
        count = 0;
    }

    public bool Contains(T item)
    {
        if (Head.Values is null)
            return false;
        CollectionLinkerEnumerator collectionLinkerEnumerator = new(Head);
        if (item is null) 
        {

            while (collectionLinkerEnumerator.MoveNext())
            {
                T value = collectionLinkerEnumerator.Current;
                if (value is null)
                    return true;
            }
        }
        else
        {
            while (collectionLinkerEnumerator.MoveNext())
            {
                T value = collectionLinkerEnumerator.Current;
                if (item.Equals(value))
                    return true;
            }
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentOutOfRangeException(nameof(array));
        if (Head.Values is null)
            return;

        CollectionLinkerEnumerator collectionLinkerEnumerator = new(Head);
        int index = arrayIndex;
        while (collectionLinkerEnumerator.MoveNext())
        {
            T value = collectionLinkerEnumerator.Current;
            array[index++] = value;
        }
    }

    public bool Remove(ICollection<T> collection)
    {
        if (RemoveIEnumerable(collection))
        {
            count -= collection.Count;
            return true;
        }
        return false;
    }

    public bool Remove(IReadOnlyCollection<T> collection)
    {
        if (RemoveIEnumerable(collection))
        {
            count -= collection.Count;
            return true;
        }
        return false;
    }

    public bool RemoveIEnumerable(IEnumerable<T> collection)
    {
        CollectionLinkerNode node = Head;
        while (node.Values is not null)
        {
            if (node.Values.Equals(collection))
            {
                if (node.Previous!.Values is not null)
                    node.Previous!.Next = node.Next;
                if (node.Next!.Values is not null)
                    node.Next.Previous = node.Previous;
                node.Dispose();
                return true;
            }

            node = node.Next!;
        }

        return false;
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException(); // impossible
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        if (count == 0)
            return EmptyEnumerator<T>.Instance;
        if (Head.Next is null)
            return Head.Values!.GetEnumerator();

        return new CollectionLinkerEnumerator(Head);
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private sealed class CollectionLinkerNode : IDisposable
    {
        public IEnumerable<T>? Values;
        public CollectionLinkerNode? Next;
        public CollectionLinkerNode? Previous;

        public CollectionLinkerNode(IEnumerable<T>? values = null, CollectionLinkerNode? prev = null, CollectionLinkerNode? next = null)
        {
            Values = values;
            Next = next;
            Previous = prev;
        }

        public void Dispose()
        {
            Values = null;
            Next = null;
            Previous = null;
        }
    }

    private sealed class CollectionLinkerEnumerator : IEnumerator<T>
    {
        private CollectionLinkerNode head;
        private CollectionLinkerNode currentNode;
        private IEnumerator<T> currentEnumerator;

        private T current = default!;
        public T Current => current;

        object IEnumerator.Current => current!;

        public CollectionLinkerEnumerator(CollectionLinkerNode head)
        {
            Debug.Assert(head.Values is not null);
            this.head = head;
            currentNode = head;
            currentEnumerator = currentNode.Values.GetEnumerator();
        }

        public void Dispose()
        {
            head = null!;
            current = default!; 
            currentNode = null!;
        }

        public bool MoveNext()
        {
            IEnumerator<T> currentEnumerator = this.currentEnumerator;
            do
            {
                bool canMove = currentEnumerator.MoveNext();
                if (!canMove)
                {
                    CollectionLinkerNode next = currentNode.Next!;
                    if (next.Values is null)
                        return false;
                    currentEnumerator.Dispose();
                    currentEnumerator = next.Values.GetEnumerator();
                    this.currentEnumerator = currentEnumerator;
                    currentNode = next;
                    continue;
                }
                current = currentEnumerator.Current;
                break;
            } while (true);

            return true;
        }

        public void Reset()
        {
            currentNode = head;
        }
    }
}
