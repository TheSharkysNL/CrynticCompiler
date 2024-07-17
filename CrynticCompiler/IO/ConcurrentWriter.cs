using System.Collections;

namespace CrynticCompiler.IO;

public sealed class ConcurrentWriter : IReadOnlyList<ConcurrentWriterSegment>
{
    internal readonly Stream writeStream;

	private int segmentCount = 0;
	private ConcurrentWriterSegment[] segments = Array.Empty<ConcurrentWriterSegment>();

    public int Count => segmentCount;

    public ConcurrentWriter(Stream stream)
	{
		if (!stream.CanWrite)
			throw new IOException("cannot write to stream");
		writeStream = stream;
	}

    public ConcurrentWriterSegment this[int index]
    {
        get
        {
            if ((uint)index >= (uint)segmentCount)
                throw new IndexOutOfRangeException();
            return segments[index];
        }
        set
        {
            if ((uint)index >= (uint)segmentCount)
                throw new IndexOutOfRangeException();
            segments[index] = value;
        }
    }

    public ConcurrentWriterSegment CreateSegment()
    {
        ConcurrentWriterSegment segment = new(this);
        Add(segment);

        return segment;
    }

    private void SubtractSegmentPosition(int index, long subtract)
    {
        ConcurrentWriterSegment nextSegment = segments[index + 1];
        nextSegment.selfPosition -= subtract;
    }

    private void AddSegmentPosition(int index, long addition)
    {
        ConcurrentWriterSegment nextSegment = segments[index + 1];
        nextSegment.selfPosition += addition;
    }

    public ConcurrentWriterSegment InsertSegment(long position)
    {
        int count = segmentCount;
        ConcurrentWriterSegment[] array = segments;
        long currentPosition = 0;
        for (int i = 0; i < count; i++)
        {
            ConcurrentWriterSegment segment = array[i];

            if (currentPosition >= position)
            {
                ConcurrentWriterSegment insertSegment = new(this, position);
                SubtractSegmentPosition(i, currentPosition - position);
                Insert(i, insertSegment);
                return insertSegment;
            }
            currentPosition += segment.selfPosition;
        }

        ConcurrentWriterSegment newSegment = new(this, position);
        Add(newSegment);
        return newSegment;
    }

    public int IndexOf(ConcurrentWriterSegment item) =>
        Array.IndexOf(segments, item);

    private static void InsertAt(ConcurrentWriterSegment[] source, ConcurrentWriterSegment[] destination, int index, ConcurrentWriterSegment item)
    {
        source.AsSpan(0, index).CopyTo(destination);
        destination[index] = item;
        source.AsSpan(index).CopyTo(destination.AsSpan(index + 1));
    }

    private void Insert(int index, ConcurrentWriterSegment item)
    {
        int count = segmentCount;
        if ((uint)index >= (uint)count)
            throw new IndexOutOfRangeException();

        count++;
        ConcurrentWriterSegment[] array = segments;

        if (count >= array.Length)
        {
            ConcurrentWriterSegment[] tempArray = new ConcurrentWriterSegment[count * 2];

            InsertAt(array, tempArray, index, item);
            segments = tempArray;
            return;
        }

        InsertAt(array, array, index, item);
    }

    private void Add(ConcurrentWriterSegment item)
    {
        int count = segmentCount;
        int newCount = count + 1;
        ConcurrentWriterSegment[] array = segments;

        if (newCount >= array.Length)
        {
            ConcurrentWriterSegment[] tempArray = new ConcurrentWriterSegment[newCount * 2];
            array.CopyTo(tempArray, 0);
            array = tempArray;
            segments = array;
        }

        array[count] = item;
        segmentCount = newCount;
    }

    //public void Clear()
    //{
    //    Array.Clear(segments);
    //    segmentCount = 0;
    //}

    public bool Contains(ConcurrentWriterSegment item) =>
        IndexOf(item) != -1;

    public void CopyTo(ConcurrentWriterSegment[] array, int arrayIndex)
    {
        int count = segmentCount;
        int arrayLength = array.Length;
        if ((uint)arrayIndex >= (uint)arrayLength)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (arrayLength - arrayIndex <= count)
            throw new ArgumentException("capacity of array is too small", nameof(array));

        segments.CopyTo(array, arrayIndex);
    }

    internal bool Remove(ConcurrentWriterSegment item)
    {
        int index = IndexOf(item);
        if (index == -1)
            return false;

        RemoveAt(index);
        return true;
    }

    internal void RemoveAt(int index)
    {
        int count = segmentCount;
        if ((uint)index >= (uint)count)
            throw new IndexOutOfRangeException();

        AddSegmentPosition(index, segments[index].selfPosition);

        count--;
        ConcurrentWriterSegment[] array = segments;
        Array.Copy(array, index + 1, array, index, count - index);
        array[count] = null!;
    }

    public IEnumerator<ConcurrentWriterSegment> GetEnumerator()
    {
        foreach (ConcurrentWriterSegment segment in segments)
            yield return segment;
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

