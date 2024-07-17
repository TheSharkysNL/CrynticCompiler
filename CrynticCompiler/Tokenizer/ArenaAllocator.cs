using System.Diagnostics;

namespace CrynticCompiler.Collections;

file sealed class ListComparer : IComparer<(int arrayIndex, int arrayLength)>
{
    public static ListComparer Instance = new();
    
    public int Compare((int arrayIndex, int arrayLength) x, (int arrayIndex, int arrayLength) y) =>
        y.arrayLength.CompareTo(x.arrayLength);
}

public sealed class ArenaAllocator<T>
{
    private T[][] arrays;
    private int count;
    private SortedList<(int arrayIndex, int arrayLength)> positions;

    private readonly int capacityPerArray;
    private const int DefaultCapacityPerArray = 4096;

    public ArenaAllocator()
        : this(0)
    { }

    public ArenaAllocator(int capacity, int capacityPerArray = DefaultCapacityPerArray)
    {
        if (capacityPerArray < 0)
        {
            throw new ArgumentException("capacityPerArray cannot be less than zero", nameof(capacityPerArray));
        }
        
        this.capacityPerArray = capacityPerArray;
        if (capacity <= 0)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("capacity cannot be less than zero", nameof(capacity));
            }

            positions = new(ListComparer.Instance);
            arrays = [];
            return;
        }

        int arrayCount = capacity / capacityPerArray + 1;
        arrays = new T[arrayCount][];
        positions = new(arrayCount, ListComparer.Instance);
        for (int i = 0; i < arrayCount; i++)
        {
            arrays[i] = GC.AllocateUninitializedArray<T>(capacityPerArray);
            positions.Add((i, capacityPerArray));
        }
    }

    private (int arrayIndex, int arrayLength) GetLargestArray()
    {
        if (positions.Count == 0)
        {
            AddNewArray();
        }

        return positions[0];
    }

    private void SetLargestArray(int index, int arrayLength)
    {
        Debug.Assert(positions.Count > 0);
        positions[0] = (index, arrayLength);
    }
    
    private int AddNewArray(int initialPosition = 0)
    {
        int count = this.count;
        int newCount = count + 1;
        if (newCount >= arrays.Length)
        {
            T[][] temp = new T[newCount * 2][];
            arrays.CopyTo(temp, 0);
            arrays = temp;
        }
        
        this.count = newCount;
        arrays[count] = GC.AllocateUninitializedArray<T>(capacityPerArray);
        positions.Add((count, capacityPerArray - initialPosition));
        
        return count;
    }

    public Memory<T> Rent(int capacity)
    {
        if (capacity <= 0)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("capacity cannot be less than zero", nameof(capacity));
            }

            return Memory<T>.Empty;
        }
        if (capacity >= capacityPerArray)
        {
            return GC.AllocateUninitializedArray<T>(capacity); // don't add to arrays pool, not going to be used for anything
        }

        (int arrayIndex, int arrayLength) = GetLargestArray();
        if (capacity > arrayLength)
        {
            int newArrayIndex = AddNewArray(capacity);
            Debug.Assert(arrays[newArrayIndex].Length > capacity);
            return arrays[newArrayIndex].AsMemory(0, capacity);
        }

        Memory<T> memory = arrays[arrayIndex].AsMemory(capacityPerArray - arrayLength, capacity);
        SetLargestArray(arrayIndex, arrayLength - capacity);
        return memory;
    }

    public Memory<T> RentCopy(ReadOnlySpan<T> span)
    {
        Memory<T> rented = Rent(span.Length);
        span.CopyTo(rented.Span);
        return rented;
    }
}